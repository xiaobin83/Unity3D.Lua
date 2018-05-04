local Strings = {}

local _gsub = string.gsub
local _gmatch = string.gmatch
local _random = math.random
local _sub = string.sub
local _tonumber = tonumber

local _Repl = function(args)
	return function(idx)
		return args[_tonumber(idx)] or ''
	end
end
local escapeChar = string.char(4,2)	
local _Unescape = function(str)
	return _gsub(str, escapeChar, '$')
end
local _Escape = function(str)
	return _gsub(str, '%$%$', escapeChar)
end
function Strings.Format(fmt, ...)
	return _Unescape(_gsub(_Escape(fmt), '$(%d)', _Repl(table.pack(...))))
end

function Strings.Trim(str)
	return _gsub(_gsub(str, '^%s*', ''), '%s*$', '')
end

function Strings.Split(str, d, trimWhiteSpace)
	trimWhiteSpace = trimWhiteSpace or true
	local r = {}
	for match in _gmatch(str..d, '([^'..d..']-)'..d) do
		r[#r+1] = trimWhiteSpace and Strings.Trim(match) or match
	end
	return r
end

local alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890'
function Strings.Random(len)
	local s = ''
	local alphabetLen = #alphabet
	for i = 1, len do
		local t = _random(1, alphabetLen)
		s = s .. _sub(alphabet, t, t)
	end
	return s
end

return Strings
