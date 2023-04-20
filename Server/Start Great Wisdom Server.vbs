Set ws = CreateObject("Wscript.Shell")
ws.currentdirectory = "bin\Debug\net7.0"

'登录服务器组
ws.run """Great Wisdom Master"" ""Configs/Login Server Group Config.json""",0
ws.run """Great Wisdom Gate"" ""Configs/Login Server Group Config.json""",0
ws.run """Great Wisdom Http"" ""Configs/Login Server Group Config.json""",0

'游戏服务器组
ws.run """Great Wisdom Master"" ""Configs/Game Config.json""",0
ws.run """Great Wisdom Gate"" ""Configs/Game Config.json""",0
ws.run """Great Wisdom Http"" ""Configs/Game Config.json""",0
ws.run """Great Wisdom World"" ""Configs/Game Config.json""",0
ws.run """Great Wisdom Hall"" ""Configs/Game Config.json""",0
ws.run """Great Wisdom Hall"" ""Configs/Game Concurrent Hall Config.json""",0