using OpenSvip.Library;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using ToolGood.Words.Pinyin;

namespace FlutyDeer.LyricsPlugin {
    public class LyricsReference {
        private const char SEPARATOR = ';';
        public enum PositionInLine {
            START,
            END,
            MIDDLE,
        }
        public class LyricsReferenceUnit {
            public string hanzi;
            public string pinyin;
            public PositionInLine positionInLine;
            public LyricsReferenceUnit(string hanzi, string pinyin, PositionInLine positionInLine = PositionInLine.MIDDLE) {
                this.hanzi = hanzi;
                this.pinyin = pinyin;
                this.positionInLine = positionInLine;
            }
            public static bool operator ==(LyricsReferenceUnit lhs, LyricsReferenceUnit rhs) {
                return lhs.pinyin == rhs.pinyin || lhs.hanzi[0] == rhs.hanzi[0];
            }
            public static bool operator !=(LyricsReferenceUnit lhs, LyricsReferenceUnit rhs) {
                return !(lhs == rhs);
            }
        }
        public class LyricsBindedUnit: LyricsReferenceUnit {
            public LyricsBindedUnit(string hanzi, string pinyin): base(hanzi, pinyin, PositionInLine.MIDDLE) {}
            public void assign(LyricsReferenceUnit lyricsReferenceUnit) {
                this.hanzi = lyricsReferenceUnit.hanzi;
                this.positionInLine = lyricsReferenceUnit.positionInLine;
            }
        }
        private List<LyricsReferenceUnit> lyricsReferenceUnitsSeries;
        private List<LyricsReferenceUnit> bindPinyin(
            List<string> lyricLine,
            string[] lyricLinePinyin
        ) {
            var LyricsReferenceUnitsSeries = new List<LyricsReferenceUnit>();
            for(int i = 0; i < lyricLine.Count; i++){
                var lyricCharacterPinyin = lyricLinePinyin[i];
                LyricsReferenceUnitsSeries.Add(new LyricsReferenceUnit(
                    lyricLine[i],
                    lyricCharacterPinyin,
                    i == 0 ? PositionInLine.START : i == lyricLine.Count - 1 ? PositionInLine.END : PositionInLine.MIDDLE
                ));
            }
            
            return LyricsReferenceUnitsSeries;
        }
        private List<string> generateLyricLineWithNonHanziCombined(string lyricLine) {
            var lyricLineWithNonHanziCombined = new List<string>();
            string buffer = "";
            for(int i = lyricLine.Length - 1; i >= 0; i--) {
                buffer = lyricLine[i] + buffer;
                if(WordsHelper.HasChinese(buffer)) {
                    lyricLineWithNonHanziCombined.Add(buffer);
                    buffer = "";
                }
            }
            lyricLineWithNonHanziCombined.Reverse();
            return lyricLineWithNonHanziCombined;
        }
        public LyricsReference(string lyricsText) {
            var lyricLineSeries = lyricsText.Split(SEPARATOR);
            var lyricLinePinyinSeries = new List<List<string>>();
            this.lyricsReferenceUnitsSeries = new List<LyricsReferenceUnit>();
            foreach(var lyricLine in lyricLineSeries) {
                var lyricLineWithNonHanziCombined = generateLyricLineWithNonHanziCombined(lyricLine);
                var lyricLineReferenceSeries = bindPinyin(
                    lyricLineWithNonHanziCombined,
                    PinyinUtils.GetPinyinSeries(lyricLineWithNonHanziCombined)
                );
                lyricsReferenceUnitsSeries.AddRange(lyricLineReferenceSeries);
            }
        }
        private List<LyricsBindedUnit> section = new List<LyricsBindedUnit>();
        public LyricsBindedUnit[] generateLyricBindedUnitSeries(List<Note> lyricsNoteSeries) {
            var lyricsBindedUnitSeries = new LyricsBindedUnit[lyricsNoteSeries.Count];
            for(int i = 0; i < lyricsNoteSeries.Count; i++) {
                lyricsBindedUnitSeries[i] = new LyricsBindedUnit(lyricsNoteSeries[i].Lyric, lyricsNoteSeries[i].Pronunciation);
            }
            int lastResult = -1;
            foreach(var lyricsBindedUnit in lyricsBindedUnitSeries) {
                for(var i = lastResult + 1; i < lyricsReferenceUnitsSeries.Count; i++) {
                    if(lyricsBindedUnit == lyricsReferenceUnitsSeries[i]) {
                        lyricsBindedUnit.assign(lyricsReferenceUnitsSeries[i]);
                        lastResult = i;
                        break;
                    } else if(lyricsReferenceUnitsSeries[i].positionInLine == PositionInLine.END) {
                        break;
                    }
                }
            }
            return lyricsBindedUnitSeries;
        }
    }
}