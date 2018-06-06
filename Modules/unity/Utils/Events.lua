local Events = {}
local Delegate = require 'unity.Utils.Delegate'

--[[
Events.shopCommand(xxx)
Events.shopCommand:Add()
]]

local events = {}

local meta = {
	__index = function(_, name)
		local evt = events[name]
		if not evt then
			evt = Delegate()
			events[name] = evt
		end
		return evt
	end,
	__newindex = function(_1, _2, _3)
		assert(false, 'forbid')
	end
}

setmetatable(Events, meta)

return Events 
