
-- https://blog.acolyer.org/2015/11/23/hashed-and-hierarchical-timing-wheels/
-- Scheme 6

local Timer = {}
Timer.__index = Timer

local Time = require 'unity.Utils.Time'

local kMaxIntervalShift = 4  --> 1 << kMaxIntervalShift seconds
local kMaxSlots = 1 << kMaxIntervalShift
local kSlotsMask = kMaxSlots - 1


local _floor = math.floor
local _ipairs = ipairs

--------------------------------------------------------------------------------
-- global timer
local timerObject 
local _Init = function()
	if not timerObject then
		timerObject = Timer.NewObject('global')
		Unity.GameObject.DontDestroyOnLoad(timerObject.gameObject)
	end
end

function Timer.StaticAfter(seconds, event, obj)
	_Init()
	timerObject:After(seconds, event, obj)
end

function Timer.StaticReset()
	if timerObject then
		timerObject:Reset()
	end
end

--------------------------------------------------------------------------------

function Timer.NewObject(name)
	local timerObject = Unity.GameObject('_TimerObject_'.. (name or 'noname'))
	return Unity.lua.AddScript(timerObject, 'unity.Utils.Timer')
end

function Timer.NewTimer()
	local timer = setmetatable({}, Timer)
	timer:Awake()
	return timer
end

--------------------------------------------------------------------------------

function Timer:Awake()
	self:Reset()
end

function Timer:After(seconds, event, obj)
	self.toArrange[#self.toArrange + 1] = {Time.RS() + seconds, event, obj}
end

function Timer:Reset()
	self.origin = Time.RS()
	self.pointer = 0
	self.slots = {}
	for i = 1, kMaxSlots do
		self.slots[i] = {}
	end
	self.toArrange = {}
end

function Timer:Update()
	local cur = Time.RS() 
	if #self.toArrange > 0 then
		for _, a in _ipairs(self.toArrange) do
			local expire = a[1]
			local s = _floor(expire - self.origin)
			if s < 0 then s = 0 end
			local slot = (self.pointer + s) & kSlotsMask
			local events = self.slots[slot+1]
			events[#events + 1] = a
		end
		self.toArrange = {}
	end
	local advance = _floor(cur - self.origin)
	for i = 0, advance do
		local slot = (self.pointer + i) & kSlotsMask
		local events = self.slots[slot + 1]
		if #events > 0 then 
			local eventsNotFired = {}
			for _, evt in _ipairs(events) do
				if cur + i >= evt[1] then
					evt[2](evt[3])
				else
					-- not fired
					eventsNotFired[#eventsNotFired + 1] = evt
				end
			end
			self.slots[slot + 1] = eventsNotFired
		end
	end
	self.origin = self.origin + advance
	self.pointer = self.pointer + advance
end

return Timer
