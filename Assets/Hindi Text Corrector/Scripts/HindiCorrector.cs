using System.Collections.Generic;

public class HindiCorrector
{
    private static string[] hindi_letters = new string[]
    {
        "‘", "’", "“", "”", "(", ")", "{", "}", "=", "।", "?", "-", "µ", "॰", ",", ".", "् ",
        "०", "१", "२", "३", "४", "५", "६", "७", "८", "९", "x",

        ":",

        "ल्म",

        "ङ",

        "ऩ",

        "ऱ",

        "य़",

        "ग़",

        "ड़",

        "ढ़",

        "ख़्य", "ख़्", "ख़",

        "क़्य", "क़्", "क़",

        "फ़्", "फ़",

        "ज़्य", "ज़्", "ज़",

        "त्त्", "त्त", "क्त", "दृ", "कृ",

        "ह्न", "ह्य", "हृ", "ह्म", "ह्र", "ह्", "द्द", "क्ष्", "क्ष", "त्र्", "त्र", "ज्ञ",
        "छ्य", "ट्य", "ठ्य", "ड्य", "ढ्य", "द्य", "द्व",
        "श्र", "ट्र", "ड्र", "ढ्र", "छ्र", "क्र", "फ्र", "द्र", "प्र", "ग्र", "रु", "रू",
        "्र",

        "ओ", "औ", "आ", "अ", "ई", "इ", "उ", "ऊ", "ऐ", "ए", "ऋ",

        "क्", "क", "क्क", "ख्", "ख", "ग्", "ग", "घ्", "घ", "ङ",
        "चै", "च्", "च", "छ", "ज्", "ज", "झ्", "झ", "ञ",

        "ट्ट", "ट्ठ", "ट", "ठ", "ड्ड", "ड्ढ", "ड", "ढ", "ण्", "ण",
        "त्", "त", "थ्", "थ", "द्ध", "द", "ध्", "ध", "न्", "न",

        "प्", "प", "फ्", "फ", "ब्", "ब", "भ्", "भ", "म्", "म",
        "य्", "य", "र", "ल्", "ल", "ळ", "व्", "व",
        "श्", "श", "ष्", "ष", "स्", "स", "ह",

        "ऑ", "ॉ", "ो", "ौ", "ा", "ी", "ु", "ू", "ृ", "े", "ै",
        "ं", "ँ", "ः", "ॅ", "ऽ", "् ", "्", "़"
    };

    private static string[] replace_letters = new string[]
    {
        "^", "*", "Þ", "ß", "¼", "½", "¿", "À", "¾", "A", "\\", "&", "&", "Œ", "]", "-", "~ ",
        "å", "ƒ", "„", "…", "†", "‡", "ˆ", "‰", "Š", "‹", "Û",

        "%",

        "Ye",

        "³",

        "u+",

        "j+",

        ";+",

        "x+",

        "M+",

        "<+",

        "[+;", "[+", "[k+",

        "D+;", "D+", "d+",

        "¶+", "Q+",

        "T+;", "T+", "t+",

        "Ù", "Ùk", "Dr", "–", "—",

        "à", "á", "â", "ã", "ºz", "º", "í", "{", "{k", "«", "=", "K",
        "Nî", "Vî", "Bî", "Mî", "<î", "|", "}",
        "J", "Vª", "Mª", "<ªª", "Nª", "Ø", "Ý", "æ", "ç", "xz", "#", ":",
        "z",

        "vks", "vkS", "vk", "v", "bZ", "b", "m", "Å", ",s", ",", "_",

        "D", "d", "ô", "[", "[k", "X", "x", "?", "?k", "³",
        "pkS", "P", "p", "N", "T", "t", "÷", ">", "¥",

        "ê", "ë", "V", "B", "ì", "ï", "M", "<", ".", ".k",
        "R", "r", "F", "Fk", ")", "n", "/", "/k", "U", "u",

        "I", "i", "¶", "Q", "C", "c", "H", "Hk", "E", "e",
        "¸", ";", "j", "Y", "y", "G", "O", "o",
        "'", "'k", "\"", "\"k", "L", "l", "g",

        "v‚", "‚", "ks", "kS", "k", "h", "q", "w", "`", "s", "S",
        "a", "¡", "%", "W", "·", "~ ", "~", "+"
    };

    public static string GetCorrectedHindiText(string unicode_substring)
    {
        int array_one_length = hindi_letters.Length;
        string modified_substring = unicode_substring;

        int position_of_quote = modified_substring.IndexOf("'");
        while (position_of_quote >= 0)
        {
            modified_substring = ReplaceFirstOccurrence(modified_substring, "'", "^");
            modified_substring = ReplaceFirstOccurrence(modified_substring, "'", "*");
            position_of_quote = modified_substring.IndexOf("'");
        }

        int position_of_Dquote = modified_substring.IndexOf("\"");
        while (position_of_Dquote >= 0)
        {
            modified_substring = ReplaceFirstOccurrence(modified_substring, "\"", "ß");
            modified_substring = ReplaceFirstOccurrence(modified_substring, "\"", "Þ");
            position_of_Dquote = modified_substring.IndexOf("\"");
        }

        var position_of_f = modified_substring.IndexOf("ि");
        while (position_of_f != -1)
        {
            var character_left_to_f = modified_substring[position_of_f - 1];

            modified_substring = modified_substring.Replace(character_left_to_f + "ि", "f" + character_left_to_f);

            while (modified_substring.Contains("्" + "f" + character_left_to_f))
            {
                var index = modified_substring.IndexOf("्" + "f" + character_left_to_f);
                modified_substring = modified_substring.Replace(
                    modified_substring[index - 1] + "्" + "f" + character_left_to_f,
                    "f" + modified_substring[index - 1] + "्" + character_left_to_f);
            }

            position_of_f = modified_substring.IndexOf("ि", position_of_f + 1);
        }

        string set_of_matras = "ािीुूृेैोौं:ँॅ";

        modified_substring += "  ";

        var position_of_half_R = modified_substring.IndexOf("र्");
        while (position_of_half_R > 0)
        {
            var probable_position_of_Z = position_of_half_R + 2;
            if (modified_substring[probable_position_of_Z + 1] == '्')
            {
                probable_position_of_Z = probable_position_of_Z + 2;
            }

            var character_right_to_probable_position_of_Z = modified_substring[probable_position_of_Z + 1];

            while (set_of_matras.IndexOf(character_right_to_probable_position_of_Z) != -1)
            {
                probable_position_of_Z = probable_position_of_Z + 1;
                character_right_to_probable_position_of_Z = modified_substring[probable_position_of_Z + 1];
            }

            var string_to_be_Replaced = modified_substring.Substring(position_of_half_R + 2,
                (probable_position_of_Z - position_of_half_R - 1));
            modified_substring = modified_substring.Replace("र्" + string_to_be_Replaced, string_to_be_Replaced + "Z");
            position_of_half_R = modified_substring.IndexOf("र्");
        }

        modified_substring = modified_substring.Substring(0, modified_substring.Length - 2);
        for (int input_symbol_idx = 0; input_symbol_idx < array_one_length; input_symbol_idx++)
        {
            if (modified_substring.Contains(hindi_letters[input_symbol_idx]))
            {
                modified_substring = modified_substring.Replace(hindi_letters[input_symbol_idx],
                    replace_letters[input_symbol_idx]);
            }
        }


        var stringsToWrap = new List<string>
        {
            "GDP",
            "AQI"
        };

        // if modified_substring contains any of the stringsToWrap, wrap them in <font="CD_Kruti_Dev_010 SDF"> </font>

        foreach (var stringToWrap in stringsToWrap)
        {
            // get all instances for string
            var indices = new List<int>();
            var index = modified_substring.IndexOf(stringToWrap);
            while (index >= 0)
            {
                indices.Add(index);
                index = modified_substring.IndexOf(stringToWrap, index + 1);
            }

            // Process in reverse order to avoid index shift
            for (int j = indices.Count - 1; j >= 0; j--)
            {
                var i = indices[j];
                modified_substring = modified_substring.Insert(i + stringToWrap.Length, "</font>");
                modified_substring = modified_substring.Insert(i, "<font=\"Hind\">");
            }
        }

        return modified_substring;
    }

    public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
    {
        int Place = Source.IndexOf(Find);
        if (Place == -1)
            return Source;
        string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
        return result;
    }
}