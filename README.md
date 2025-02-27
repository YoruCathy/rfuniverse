# RFUniverse

RFUniverse是基于Unity开发的用于机器人仿真和强化学习的平台，主要有三个功能模块：

[Python接口](https://github.com/mvig-robotflow/rfuniverse/blob/main/RFUniverse%20API.md)：Python封装的通信接口 [![Pypi](https://img.shields.io/pypi/v/pyrfuniverse.svg)](https://pypi.org/project/pyrfuniverse/)

Unity端Player：接收python端消息并执行仿真

[Unity端Editor](https://github.com/mvig-robotflow/rfuniverse/blob/main/RFUniverse%20Editor%20User%20Manual.pdf)：用于搭建或编辑仿真场景

---

按照以下步骤配置并通过发布版运行示例场景

1. 创建conda环境，激活并安装pyrfuniverse包：
   
   ```
   conda create -n rfuniverse python=3.9 -y
   conda activate rfuniverse
   pip install pyrfuniverse
   ```

2. 下载RFUniverse可执行程序并解压：[RFUniverse Releases](https://github.com/mvig-robotflow/rfuniverse/releases)
   
   解压完成后运行一次程序，进入场景后即可关闭：
   
   Linux：`RFUniverse.x86_64`
   
   Windows：`RFUniverse.exe`

3. Clone pyrfuniverse仓库，切换到与RFUniverse Release相同的Tag：
   
   ```
   git clone https://github.com/mvig-robotflow/pyrfuniverse.git
   ```

4. 运行 `pyrfuniverse/AtomicActions`，`pyrfuniverse/Test`下任意python脚本(某些脚本可能需要安装pybullet，open3d等比较大的包)
   
   ```
   cd pyrfuniverse/Test
   python test_pick_and_place.py
   ```

---

##### Test目录

| name                                   | feature                 |
| -------------------------------------- | ----------------------- |
| test_active_depth                      | 红外深度                    |
| test_articulation_ik                   | 原生IK                    |
| test_camera_image                      | 相机截图示例                  |
| test_custom_message                    | 自定义消息和动态消息              |
| test_grasp_pose                        | Franka抓点预览              |
| test_grasp_sim                         | Franka抓取测试              |
| test_humanbody_ik                      | 人体IK接口示例                |
| test_load_mesh                         | obj模型文件导入为刚体            |
| test_load_urdf                         | urdf文件导入                |
| test_pick_and_place                    | 基础接口和原生IK驱动的抓取示例        |
| test_pick_and_place_gym                | 基础接口和原生IK驱动的抓取训练任务示例    |
| test_point_cloud                       | 使用图像宽高和fov获取深度图并转换点云的示例 |
| test_point_cloud_with_intrinsic_matrix | 使用相机内参获取深度图并转换点云的示例     |

---

##### 进入Edit模式

启动RFUniverse时添加参数<-edit>可进入Edit模式

Linux:

```
RFUniverse.x86_64 -edit
```

Windows:

```
RFUniverse.exe -edit
```

---

## Unity源工程与SDK使用说明

*以下步骤说明将默认你对UnityEditor有一定了解*

如果你想要在RFUniverse中加入自己的定制资源或功能，可以在RFUniverse开源工程基础上添加

如果你想要为自己的项目添加RFUniverse功能，可以导入[RFUniverse Core SDK](https://github.com/mvig-robotflow/rfuniverse/releases)

**工程中若出现缺失插件的报错，可重启工程后使用菜单RFUniverse/CheckPlugins修复**

---

##### 插件与资源补足

打开RFUniverse工程或导入RFUniverse Core SDK后需要自行补充第三方插件和资源，以开启其中的功能

- (可选)[Obi](https://assetstore.unity.com/publishers/5170)：Softbody，Cloth，Fluid等物理仿真插件
- (可选)[BioIK](https://assetstore.unity.com/packages/tools/animation/bio-ik-67819)：关节IK解算插件

**请将插件放入Plugins目录，导入插件后使用菜单RFUniverse/CheckPlugins修复依赖**

---

##### Assets目录结构

* AddressableAssetsData：Unity可寻址资源系统固定目录，管理资源地址和资源打包配置
* Assets：资源目录，包含所有动态加载资源，**如果不需要使用内置的模型和机器人资源，可以将其删除**
  * Model：模型/贴图/材质等资源
  * PhysicalMaterials：物理材质
  * Prefab：预制体，分配Addressable地址用于资源加载
* Extend： 各种扩展Attr模块
* Plugins：扩展插件目录，请将OBI，BioIK等插件放置在此目录下
* RFUniverse：RFUniverse Core 核心功能资源及脚本
* StreamingAssets：配置文件保存目录
  * SceneData：场景JSON文件的保存目录
* TextMesh Pro：TMP UI 资源

---

##### Scene场景

* RFUniverse/First.unity：发布程序运行的首个场景，在该场景接收命令行参数后跳转至其他场景
* RFUniverse/Empty.unity：Player模式场景
* EditMode/Edit.unity：Editor模式场景

---

##### 工程下运行示例场景

pyrfuniverse/Test目录的功能示例即可以再Release下运行，也可以在UnityEditor中运行

先运行一次Empty场景后退出，随后运行某个python脚本，再在UnityEditor中运行Empty场景即可

---

##### 核心功能

###### Agent

是Python与Unity进行通信的基础

###### Attributes

Attr是RFUniverse中物体的基本单元，所有的物体都是基于BaseAttr派生而来，如GameObjectAttr，RigidbodyAttr，ControllerAttr， CmaeraAttr等

```mermaid
graph TD
Base-->BaseCamera
Base-->...
Base-->GameObject
GameObject-->Collider
BaseCamera-->Camera
BaseCamera-->HDRPCamera
Collider-->Rigidbody
Collider-->Controller
```

其中：

BaseAttr提供了基础的物体加载创建删除移动等属性

GameObject扩展物体简单的视觉效果修改

ColldierAttr扩展物体碰撞体的修改功能

Rigidbody扩展物体的刚体属性

Controller扩展机械臂关节体的操作

CameraAttr扩展相机图像抓取功能

###### Manager

Manager负责接受和发送不同类型的数据，每一个Manager有独立的channel与python保持通信，在运行过程中通过channel接受或发送数据。RFUniverser中有两个重要的Manager：

- AssetManager：负责环境中通用的接口和数据的发送
- InstanceManager：负责分发和收集面向不同Attr的接口和数据

---

##### 搭建一个简单场景

在RFUniverse中，可以将物体按照规则配置成Prefab并在运行时通过python接口动态加载物体，也可以提前搭建好固定的场景来与python通信，两种方式的选择在于你是否需要在一个发布版中运行不同的环境，大部分情况下在UnityEditor中提前搭建场景更简单快捷。

###### 搭建场景的基本流程：

1. 复制一份Empty场景，在此基础上添加自己的物体。
   
   或者将RFUniverse/Assets/Prefab/RFUniverse导入现有场景，同时移除场景中原本的MainCamera和Directional Light
2. 为需要通信的物体添加BaseAttr脚本，手动设置不同的ID，保证ID没有重复
3. 参照pyrfuniverse/Test编写python脚本，通过ID读取物体上的信息并调用物体上的接口

---

##### 添加定制化接口

RFUniverse还在不断开发升级维护中，更新比较频繁，为了保证核心代码的更新不影响二次开发，添加了接口和数据通信的扩展模块。如果你需要在RFUniverse中添加自己的功能，请遵循以下步骤和限制，以保证RFUnivers后续更新不会与你添加的代码冲突

接口分为全局接口(AssetManager)和面向对象接口(InstanceManager)

* ###### 扩展Attr面向对象接口/属性
  
  在pyrfuniverse工程中，参照pyrfuniverse/attributes/custom_attr.py，在同目录下新建脚本，添加新增的消息读取代码和新接口，传入参数类型为 `dict`，返回值类型为 `OutgoingMessage`。并在pyrfuniverse/attributes/\_\_init\_\_.py中添加 `import`，并加入 `__all__`
  
  在Unity工程中，参照RFUniverse/Scripts/Attributes/CustomAttr.py脚本，新建脚本，继承 `BaseAttr`或其他派生类，重写Type属性值与python脚本_attr前半段命名相同，重写 `CollectData`在其中写入新增的数据，重写 `AnalysisMsg`添加接口实现函数。

* ###### 扩展全局接口/属性
  
  在pyrfuniverse工程中，修改pyrfuniverse/rfuniverse_channer/asset_channer_ext.py脚本，参照现有代码，添加新增的消息读取代码和新接口，同样传入类型为 `dict`，返回类型为 `OutgoingMessage`
  
  在Unity工程中，修改AssetManagerExt.cs脚本，在 `AnalysisMsg`方法的 `switch`块中添加分支，并添加接口接收函数。数据发送可以在任意位置调用 `AssetManager.Instance.channel.SendMetaDataToPython(sendMsg);`
  
  

定制化接口的具体添加示例请看pyrfuniverse/Test/test_custom_message.py

---

##### 动态消息接口

除了固定参数的接口外，AssetManager还支持发送动态消息进行双向数据通信，更加灵活方便

* **Python->Unity**
  
  C#:
  
  `AssetManger.Instance.AddListener(string message, Action<IncomingMessage> action);`
  
  传入消息名称和消息接收函数开启监听，接受函数的传入参数类型为 `IncomingMessage`
  
  Python:
  
  `env.asset_channel.SendMessage(self, message: str, *args)`
  
  传入消息名称和任意数量的数据进行发送

* **Unity->Python**
  
  Python:
  
  `env.asset_channel.AddListener(self, message: str, fun)`
  
  传入消息名称和消息接收函数开启监听，接受函数的传入参数类型为 `IncomingMessage`
  
  C#:
  
  `AssetManger.Instance.SendMessage(string message, params object[] objects);`
  
  传入消息名称和任意数量的数据进行发送
  
  

*请注意，动态消息必须保证接收函数中从 `IncomingMessage`读取数据的类型和顺序与发送消息时传入的类型和顺序相同，否则程序会报错*

动态消息接口的具体使用示例请看pyrfuniverse/Test/test_custom_message.py
