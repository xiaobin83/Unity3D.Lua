local ObjectPool = {}

local Unity = require 'unity.Unity'
local _R = require 'unity.Utils.ResMgr'
local Timer = require 'unity.Utils.Timer'
local Math = require 'unity.Math'

local freeList = {}
local allocList = {}

local kAction_Obtain = 0
local kAction_ObtainFromPrefab = 1
local kAction_Release = 2

function ObjectPool.NativePoolDelegate(action, ...)
	if action == kAction_Obtain then
		return ObjectPool.Obtain(...)
	elseif action == kAction_ObtainFromPrefab then
		return ObjectPool.Obtain('from_prefab', ...)
	elseif action == kAction_Release then
		ObjectPool.Release(...)
	end
end

local poolTransform 
local _GetPoolTransform = function()
	if not poolTransform then
		local go = Unity.GameObject('_PoolObject')
		Unity.GameObject.DontDestroyOnLoad(go)
		poolTransform = go.transform
	end
	return poolTransform
end

function ObjectPool.Obtain(type, uri, ...)
	local list = freeList[type]
	if not list then
		list = {}
		freeList[type] = list
	end
	local prefab
	if type == 'from_prefab' then
		prefab = uri
		uri = uri:GetInstanceID()
	end
	local objList = list[uri]
	if not objList then
		objList = {}
		list[uri] = objList 
	end
	local lastObjIdx = #objList
	local obj
	if lastObjIdx > 0 then
		obj = objList[lastObjIdx]
		local posU, rotU, trans = ...
		if posU then
			obj.transform.position = posU
		end
		if rotU then
			obj.transform.rotation = rotU
		end
		if trans then
			obj.transform:SetParent(trans, true)
		end
		objList[lastObjIdx] = nil
	else
		if prefab then
			obj = Unity.GameObject.Instantiate(prefab, ...)
		else
			obj = _R(type, uri, ...)
			if not obj then return nil end
		end
	end
	allocList[obj:GetInstanceID()] = {obj, type, uri}
	return obj
end


local _Release = function(obj)
	if obj == nil then return end

	local id = obj:GetInstanceID()
	local objTuple = allocList[id]
	if objTuple then

		obj:SetActive(false)
		obj.transform:SetParent(_GetPoolTransform(), false)

		local obj_, type, uri = unpack(objTuple)
		assert(obj_ == obj)
		obj:SetActive(false)
		allocList[id] = nil
		local objList = freeList[type][uri] 
		objList[#objList + 1] = obj
	else
		_LogW('not allocated by pool. only destroy it')
		Unity.GameObject.Destroy(obj)
	end
end

function ObjectPool.Release(obj, delay)
	if obj == nil then return end
	delay = delay or 0
	if delay > 0 then
		Timer.StaticAfter(delay, _Release, obj)
	else
		_Release(obj)
	end
end

function ObjectPool.CleanUnused()
	if poolTransform then
		Unity.GameObject.Destroy(poolTransform.gameObject)
		poolTransform = nil 
	end
	freeList = {}
end

function ObjectPool.Clean()
	ObjectPool.CleanUnused()
	allocList = {}
end

return ObjectPool 
