# JocationPlus iOS GPS Location Mocking Editor

#### Forked from quxiaozha(https://github.com/quxiaozha/JocationPlus). Thanks for the hardwork and keeping it open source :)
#### I'll add the latest driver, modify some functions, as well as develop new ones for my personal use.

**TODO list**:
- [ ] Edit current GPS format input and display to Lat/Lon
- [ ] Convert current speed display to km/h
- [ ] Add the option to choose between running or teleporting
- [ ] Change current map to Google Map
- [ ] Implement a way to read GPX file

#### To run: git clone then build the project using VS. Run the .exe file. 

#### History version

- v1.3  增加**速度**和**方向**按钮，支持**键盘上下左右**操作，支持**手工输入**经纬度
- v1.4 增加**匀速**移动功能
- v1.5 增加地点**保存**、**删除**功能、支持保存**当前坐标** 
- v1.6 修复百度地图经纬度偏移，当通过**自带的地图**选取坐标后，会将**BD-09**坐标转换为**WGS-84**坐标，但是不对转换手工输入的坐标，因为我不知道你输入的是什么坐标系~
- v1.7 新增**四个方向**，支持**数字键盘**，调整UI，感谢钟小懒同学的UI设计，有小彩蛋哦~
- v1.7.1 打开地图时默认顺序为:**当前位置>列表位置>IP位置**，解决连接同一wifi下手机问题，禁用还原定位按钮，日志面板禁止编辑且自动滚动到最底端



#### TIPS：
- If encounter ``` An Lockdown error occurred. The error code was InvalidService.```errors，Please refer to [this](https://github.com/quxiaozha/JocationPlus/issues/2)

