from typing import Tuple
import json

from model import OpenSvipModel

import clr

# 导入 .net 基础库
from System.IO import FileStream, FileMode, FileAccess
from System.Runtime.Serialization.Formatters.Binary import BinaryFormatter
clr.AddReference(r'C:\Users\YQ之神\AppData\Local\warp\packages\XStudioSinger_2.0.0_beta2.exe\SingingTool.Model.dll')
from SingingTool.Model import AppModel


def read_svip(path: str) -> Tuple[str, AppModel]:
    reader = FileStream(path, FileMode.Open, FileAccess.Read)
    version_ascii_list = []
    reader.ReadByte()
    for _ in range(4):
        version_ascii_list.append(reader.ReadByte())
    reader.ReadByte()
    for _ in range(5):
        version_ascii_list.append(reader.ReadByte())
    version = ''.join([chr(ch) for ch in version_ascii_list])
    model = BinaryFormatter().Deserialize(reader)
    reader.Close()
    return version, model


def write_svip(path: str, version: str, model: AppModel):
    version_ascii_list = [4] + [ord(ch) for ch in version[:4]] + [5] + [ord(ch) for ch in version[4:]]
    writer = FileStream(path, FileMode.Create)
    writer.Write(version_ascii_list, 0, 11)
    BinaryFormatter().Serialize(writer, model)
    writer.Flush()
    writer.Close()
    writer.Dispose()


def svip_to_json(input_path: str, output_path: str, indent=None):
    v, m = read_svip(input_path)
    model = OpenSvipModel().decode(v, m)
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(json.dumps(model, default=lambda obj: obj.__dict__, ensure_ascii=False, indent=indent))


def json_to_svip(input_path: str, output_path: str):
    with open(input_path, 'r', encoding='utf-8') as f:
        model = OpenSvipModel().dedict(json.load(f))
    v, m = model.encode()
    write_svip(output_path, v, m)


if __name__ == '__main__':
    svip_to_json('test/黏黏黏黏.svip', 'test/黏黏黏黏.json', indent=2)
    json_to_svip('test/黏黏黏黏.json', 'test/out.svip')
    pass
