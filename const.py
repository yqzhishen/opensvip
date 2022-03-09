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
