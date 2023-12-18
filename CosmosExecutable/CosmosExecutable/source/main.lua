local sha1 = require'sha1'.sha1

function main()
	print("Hashing SHA1 of " .. arg[1])
    print(sha1(arg[1]))
end