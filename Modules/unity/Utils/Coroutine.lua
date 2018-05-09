local Coroutine = {}
local Time = require 'unity.Utils.Time'

function Coroutine.After(be, due, func)
	local timeDue = Time.RS() + due
	local co = coroutine.create(function() 
		while Time.RS() < timeDue do
			coroutine.yield()
		end
		func()
	end)
	be:StartLuaCoroutine(co)
end

function Coroutine.Every(be, dur, func)
	local nextDue = Time.RS() + dur
	local co = coroutine.create(function()
		while true do
			local c = Time.RS()
			while c < nextDue do
				coroutine.yield()
				c = Time.RS()
			end
			nextDue = c + dur
			local shouldBreak = func()
			if shouldBreak then
				break
			end
		end
	end)
	be:StartLuaCoroutine(co)
end

function Coroutine.When(be, cond, func)
	local c, shouldBreak = cond()
	if shouldBreak then	return end
	if c then
		func()
		return
	end
	local co = coroutine.create(function()
		c, shouldBreak = cond()
		if shouldBreak then return end
		while c do 
			coroutine.yield()
			c, shouldBreak = cond()
			if shouldBreak then
				return
			end
		end
		func()
	end)
	be:StartLuaCoroutine(co)
end

return Coroutine
