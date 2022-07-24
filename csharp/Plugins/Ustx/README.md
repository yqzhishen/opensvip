# OpenUTAU ustx插件
适用于[OpenSVIP](https://openvpi.github.io/home/)转换器的OpenUTAU ustx插件。

作者：[Oxygen Dioxide](https://github.com/oxygen-dioxide)

如果你发现了bug，请在[我的Github仓库](https://github.com/oxygen-dioxide/opensvip/issues)提出反馈。

## 下载
[从Github下载](https://github.com/oxygen-dioxide/opensvip/releases)

## 支持特性
### ustx输入
* 支持的数据：
  * 曲速、拍号
  * 音轨
    * 同一音轨上的多个区段将被合并，音轨名称为第一个区段的名称。
  * 音符
  * 音高曲线
    * 锚点、手绘音高线、颤音，将合并转化。

### ustx输出
* 支持的数据：
  * 音轨、音符
  * 曲速、拍号
    * 暂不支持变速曲自动转换，仅保留输入文件中的第一个曲速。

## 开源软件声明
本插件基于以下开源软件：
* [OpenUTAU](https://github.com/stakira/OpenUtau)（[MIT协议](https://github.com/stakira/OpenUtau/blob/master/LICENSE.txt)）
* [YamlDotNet](https://github.com/aaubry/YamlDotNet) （[MIT协议](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt)）