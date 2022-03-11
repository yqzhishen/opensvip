import json
import re


class OpenSvipSingers:
    singers: dict
    with open('OpenSvipSingers.json', 'r', encoding='utf-8') as f:
        singers = json.load(f)

    @staticmethod
    def get_name(id_: str):
        if id_ in OpenSvipSingers.singers:
            return OpenSvipSingers.singers[id_]
        if re.match(r'[FM]\d+', id_) is not None:
            return f'$({id_})'
        return ''

    @staticmethod
    def get_id(name: str):
        for id_ in OpenSvipSingers.singers:
            if OpenSvipSingers.singers[id_] == name:
                return id_
        if re.match(r'\$\([FM]\d+\)', name) is not None:
            return name[2:-1]
        return ''


class OpenSvipReverbPresets:
    presets = {
        -1: '干声',
        0: '浮光',
        1: '午后',
        3: '月光',
        5: '水晶',
        7: '汽水',
        9: '夜莺',
        18: '大梦'
    }

    @staticmethod
    def get_name(index):
        return OpenSvipReverbPresets.presets[index]

    @staticmethod
    def get_index(name):
        for index in OpenSvipReverbPresets.presets:
            if OpenSvipReverbPresets.presets[index] == name:
                return index
        return -1


class OpenSvipNoteHeadTags:
    tags = [None, '0', 'V']

    @staticmethod
    def get_name(index):
        return OpenSvipNoteHeadTags.tags[index]

    @staticmethod
    def get_index(name):
        if name == '0':
            return 1
        elif name == 'V':
            return 2
        else:
            return 0
