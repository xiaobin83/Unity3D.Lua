local Set = {}

function Set.GetIntersection(seq1, seq2)
	local s = {}
	local r = {}
	for _, k in ipairs(seq1) do
		s[k] = 1
	end
	for _, k in ipairs(seq2) do
		if s[k] then
			r[#r + 1] = k
		end
	end
	return r 
end

function Set.Intersects(seq1, seq2)
	if #seq1 > #seq2 then
		local t = seq1
		seq1 = seq2
		seq2 = t
	end
	local s = {}
	for _, k in ipairs(seq1) do
		s[k] = 1
	end
	for _, k in ipairs(seq2) do
		if s[k] then
			return true
		end
	end
	return false
end

function Set.Merge(seq1, seq2)
	local s = {}
	for _, k in ipairs(seq1) do
		s[k] = 1
	end
	for _, k in ipairs(seq2) do
		s[k] = 1
	end
	local r = {}
	for k, _ in pairs(s) do
		r[#r + 1] = k
	end
	return r 
end


return Set
