# SnowbreakGachaExport

基于OpenCV和Tesseract的抽卡记录导出工具，设计基于尘白禁区，理论上经调整可用于任何带有抽卡记录历史展示的游戏，可以分池统计。

Snwobreak: Containment Zone Gacha Exporter, based on Tesseract, it may be used in other games with some change if you want,.

OpenCVとTesseractをベースにしたガチャ履歴のエクスポートツール、デザインはスノウブレイクをベースに、理論的にはガチャ履歴の表示があるゲームであれば、どんなゲームでも使えるように調整されている、現在はまだ開発中で、履歴の読み込みと簡単な表示機能しか完成していない、バグも多いかもしれない。

---

## Requirements

.Net 7.0 Runtime

---

## 使用

![loading-ag-1133](Document/Images/Display.png)

- **注意：程序需要以管理员身份运行**
  
  **ATTENTION: Run As Administer is Required**
  
  **注: プログラムは管理者として実行する必要があります**
  
  ---

- 从`Release`下载最新版本并解压到任意目录
  
  Download from `release`
  
  `release`から最新版をダウンロードし、任意のディレクトリに解凍する。
  
  ---

- 在游戏中打开抽卡记录第一页
  
  Open the first page of gacha log in game
  
  ゲーム内のガチャ履歴の最初のページを開く
  
  ---

- 打开`SnowbreakGachaExport.exe`，在左侧切换到设置页面，在右上角下拉框选择尘白禁区程序名（一般为`Snowbreak:xxxx`），回到首页，点击右上角的开始刷新按钮  
  
  Open `SnowbreakGachaExport.exe`, select Snwobreak: containment zone in the combo box in top-right, and click refresh
  
  `SnowbreakGachaExport.exe`を開き、右上のドロップダウンボックスでスノブレのスノブレプログラム名（通常は`Snowbreak:xxxx`）を選択し、左側のRefreshをクリックします。
  
  ---

- 此时应自动返回至游戏界面，大约1秒读取一页（10条）数据，等待自动读取翻页至最后一页，等待程序自动跳出“Finished通知”
  
  It will auto back to game, and read log about 3-5 sec per page, waiting for reading to the last page and the export doesn't click the next page button
  
  この時点で自動的にゲーム画面に戻ってデータを読み取る(約3〜5秒)、最後のページで停止までに待つ
  
  ---

- 返回程序查看记录
  
  return to the exporter it will display the log now, it will only display in the second zone
  
  プログラムに戻り、データを見る。現在、このデータは2番目の統計エリアにのみ表示されている
