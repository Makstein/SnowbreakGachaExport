# SnowbreakGachaExport

基于Opencv和Tesseract的抽卡记录导出工具，设计基于尘白禁区，理论上经调整可用于任何带有抽卡记录历史展示的游戏，目前尚在开发，只完成了读取历史记录并进行简单展示的功能，目前不能保存和导出，只能统计单一池子，并且可能有许多bug。

Snwobreak: Containment Zone Gacha Exporter, based on OpenCV and Tesseract, it may bse used in other with some change if you want, WIP now, only finished read and display gacha log, can not save or export and can only read one pool for now, may have many bugs.

## 使用

- 从Release下载最新版本并解压到任意目录 

- Download from release

- 在游戏中打开抽卡记录第一页 

- Open the first page of gacha log in game

- 打开SnowbreakGachaExport.exe，在右上角下拉框选择尘白禁区程序名（一般为Snowbreak:xxxx），点击左侧Refresh

- Open SnowbreakGachaExport.exe, select Snwobreak: containment zone in the combobox in top-right, and click refresh

- 此时应自动返回至游戏界面，大约3-5秒读取一页（10条）数据，等待自动读取翻页至最后一页，等待三到五秒至程序不再点击翻页键

- It will auto back to game, and read log about 3-5 sec per page, waitting for reading to the last page and the export doesn't click the next page button

- 返回程序查看记录，目前只会显示在第二个统计区域

- return to the exporter it will display the log now, it will only display in the second zone
