local ConnectToEditor = {}

local io = require 'unity.io'
local json = require 'rapidjson'

local conn

local _MsgH = function(msg)
	_LogE(msg .. '\n' .. debug.traceback())
	return msg
end
local _Exec = function(cmd)
	local chunk, errmsg = load('return function()\n'..cmd..'\nend\n')
	if not chunk then
		_LogE(errmsg)
		return errmsg
	end
	local f = chunk()
	local _, ret = xpcall(f, _MsgH)
	return ret
end

function ConnectToEditor.Connect()
	conn = require 'unity.connect_to_editor'
	conn.Register('exec_lua', 
		function(msg) 
			local m = json.decode(msg)
			_LogD('exec_lua ' .. string.sub(m.content, 1, 80))
			return _Exec(m.content)
		end)
	conn.Register('put_file',
		function(msg)
			local m = json.decode(msg)
			_LogD('put_file ' .. m.path)
			local dir = io.GetDirectoryName(m.path)
			if not io.DirectoryExists(dir) then
				local err = io.MakeDirectory(dir)
				if not err then
					_LogE(err)
					return
				end
			end
			local err = io.WriteAllText(m.path, m.content)
			if err then
				_LogE(err)
			end
		end)
end



return ConnectToEditor
