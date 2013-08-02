import "System"
import "System.IO"

function CreateFile()
	File.WriteAllText("testfile.txt", "lolol")
end

function DeleteFile()
	File.Delete("testfile.txt")
end