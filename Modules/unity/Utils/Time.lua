local Time = {}
local _LogT, _LogD, _LogI, _LogW, _LogE = require('unity.Debug').GetLogFuncs('[TIME]')
local Unity = require 'unity.Unity'
local UnityTime = Unity.Time
local Misc = require 'unity.misc'

Time.kMinute = 60
Time.kHour = Time.kMinute * 60
Time.kDay = Time.kHour * 24
Time.kWeek = Time.kDay * 7


local _floor = math.floor

-- realtime since startup, for measuare execution time
function Time.RS()
	return UnityTime.realtimeSinceStartup
end

function Time.UnscaledDT()
	return UnityTime.unscaledDeltaTime
end

function Time.DT()
	return UnityTime.deltaTime
end

function Time.RawTS()
	return Misc.EpochNow() 
end

function Time.GetDays(second)
	return second / Time.kDay
end

function Time.GetHours(second)
	return second / Time.kHour 
end	

function Time.GetMinues(second)
	return second / Time.kMinute
end	

-- x day y hrs z min w second
function Time.GetDifference(second)
	local d = _floor(Time.GetDays(second))
	local h = _floor(Time.GetHours(second % Time.kDay))
	local m = _floor(Time.GetMinues(second % Time.kHour))
	local s = _floor(second % Time.kMinute)
	return d, h, m, s
end




local _M = {}
_M.__index = _M

-- timestamp, used for sync between server and client
function _M:TS()
	return Misc.EpochNow() + (self.delta or 0) + self.extraDelta
end


function _M:StartSyncTime()
	if self.syncing then return end
	--_LogT(self.name .. ' StartSyncTime')
	assert(self.mode ~= 'fixed', 'fixed timer cannot be sync')
	self.timeSamples = {}
	self.syncing = true
end

function _M:RecvDelta(localTime, serverTime)
	_LogT(self.name .. ' RecvDelta ' .. localTime .. ', ' .. serverTime)
	assert(self.mode ~= 'fixed', 'fixed timer cannot be sync')
	local d = (serverTime - localTime) / 2
	if not self.delta then
		self.delta = d
	end	
	table.insert(self.timeSamples, d)
end

function _M:EndTimeSync()
	assert(self.mode ~= 'fixed', 'fixed timer cannot be sync')
	local n = #self.timeSamples
	assert(n > 2)		
	table.sort(self.timeSamples)
	local median = self.timeSamples[math.floor(n / 2)]
	local median15 = 1.5 * median
	local sum = 0
	local count = 0
	for _, d in ipairs(self.timeSamples) do
		if d - median < median15 then
			sum = sum + d
			count = count + 1
		end
	end
	if count > 0 then
		local old = self.delta
		self.delta = sum / count
		--_LogT(self.name .. ' EndTimeSync: ' .. old .. ' -> ' .. self.delta)
	else
		_LogE('sync time has problem')
	end
	self.timeSamples = false
	self.syncing = false
end

local _NewTime = function(name, mode, arg0)
	local inst = {
		name = name,
		delta = false,
		extraDelta = mode == 'extra_delta' and arg0 or 0,
		timeSamples = {},
		mode = mode,
		syncing = false,
	}
	setmetatable(inst, _M)
	return inst
end

local cachedTime = {}
function Time.New(name, mode, arg0) 
	if not name then
		return _NewTime(name, mode, arg0)
	end
	local t = cachedTime[name]
	if not t then
		t = _NewTime(name, mode, arg0)
		cachedTime[name] = t
	end
	return t
end

return Time
