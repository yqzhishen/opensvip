using System.Collections.Generic;

namespace XSAppModel.XStudio
{

    public class AppModel
    {
        public AppModel()
        {
            ProjectFilePath = "";

            tempoList = new List<SongTempo>();
            beatList = new List<SongBeat>();
            trackList = new List<ITrack>();

            quantize = 8;
            isTriplet = false;
            isNumericalKeyName = true;
            firstNumericalKeyNameAtIndex = 0;
        }

        /* Properties */
        public string ProjectFilePath;

        /* Members */
        public List<SongTempo> tempoList;
        public List<SongBeat> beatList;

        public List<ITrack> trackList;

        public int quantize;
        public bool isTriplet;
        public bool isNumericalKeyName; // The word was misspelled in the original code
        public int firstNumericalKeyNameAtIndex; // The word was misspelled in the original code
    }
}