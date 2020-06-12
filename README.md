# 2d_game_lib
## collision_and_rigid快速四叉树碰撞算法
介绍
    角色抽象为限定为半径一定的圆
    地图为闭合线段图形地图，输入为多个多边形（凹凸不限，内部是否为障碍），定义好角色半径，使用
    SomeTools.GenWalkBlockByPolys
    方法生成针对此半径角色的四叉树碰撞地图WalkBlock，这种地图是把圆和多个多边形的碰撞转化为点与直线，弧线组合的碰撞，并且直线和弧线会分布到四叉树结构中，并使用AABB包围盒射线碰撞排除
    
    WalkBlock的Qspace使用OutZones()可以递归打印branch和leaf的aabb包围盒范围
    WalkBlock.CoverPoint可以判断是否碰撞到该点
    
