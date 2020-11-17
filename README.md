IDE使用jetBrains_Rider
https://www.jetbrains.com/?from=ty_game_lib
# ty_game_lib
## collision_and_rigid快速四叉树碰撞算法
简介：关于地图碰撞，刚体控制方面的库，游戏对象都是射到2d平面
WalkBlock碰撞地图
    
    角色抽象为限定为半径一定的圆
  
    地图为闭合线段图形地图，输入为多个多边形（凹凸不限，内部是否为障碍），定义好角色半径，使用
  
    SomeTools.GenWalkBlockByPolys
    
    方法生成针对此半径角色的四叉树碰撞地图WalkBlock，
    优势：这种地图是把圆和多个多边形的碰撞转化为点与直线，弧线组合的碰撞，
    并且直线和弧线会分布到四叉树结构中，并使用AABB包围盒射线碰撞排除，
    所以这样会比一般的四叉树角色之间碰撞要快
    
    WalkBlock的Qspace使用OutZones()可以递归打印branch和leaf的aabb包围盒范围
    CoverPoint可以判断是否碰撞到该点
    PushOutToPt可以简单模拟刚体碰撞效果，建议放到前端先运算一次

IBulletShape和IRawBulletShape
    
    IRawBulletShape使用GenBulletShape,配置角色半径生成IBulletShape；
    IBulletShape使用PtInShape判断角色是否被命中

## game_config
简介：配置文件专门工程
使用json作为内部资源，配置存到字典
    
    var configDictionaries = new ConfigDictionaries();
也可以使用json文件

    var configDictionaries = new ConfigDictionaries("{jsonPath}");
    //使用.net core 时有自动递归寻找功能

可以在unity中使用字符串字典来装载配置，比如有对应命名的json文件中的content中有对应配置,查看unity_sample中的脚本范例，需要用.net standard 2.0的库来使用
    
## game_stuff
简介：动作游戏玩法抽象运行逻辑，目的为多端运行,具体了解参看战斗系统介绍.pptx
## cov_path_navi
简介：2d凸多边形寻路模块，精度一般，不一定是最短的路径，但是比方格的节点少，效率能高一些
从一个WalkBlock阻挡地图对象生成一个PathTop对象
通过PathTop，设置起点，终点，即可生成一条行走目标点路径

    var pathTop = new pathTop(walblock)
    var path = pathTop.FindAPathByPoint( startPt, endPt)
    //如果维护好当前和目标所在多边形节点，则可以补充参数以免寻找所在的多边形
    var fasterPath = pathTop.FindAPathByPoint(startPt, endPt, startPolyId, endPolyId)
    //只传入起点终点多边形Id也可以找到路径,精度进一步下降
    var PathById = pathTop.FindAPathById( startPolyId, endPolyId)
    
