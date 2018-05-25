local meta = {
	__add = function(delegate, func)
		delegate.__methods[#delegate.__methods + 1] = func
		return delegate
	end,

	__sub = function(delegate, func)
		for i, f in ipairs(delegate.__methods) do
			if f == func then
				table.remove(delegate.__methods, i)
			end
		end
		return delegate
	end,

	__call = function(delegate, ...)
		for _, f in ipairs(delegate.__methods) do
			f(...)
		end
	end,

	__index = {
		Add = function(delegate, func)
			return delegate + func
		end,
		Remove = function(delegate, func)
			return delegate - func
		end
	}
}

return function()
	return setmetatable({__methods={}}, meta)
end
