# 2d_game_lib
## collision_and_rigid快速四叉树碰撞算法
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
使用json作为内部资源，配置存到字典
Models.cs由python脚本生成 //TODO 命令行调脚本，保存json和刷新Models.cs文件
// TODO 可以保存字典到二进制文件，初步测试可行
