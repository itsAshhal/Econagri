using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private GameObject tooltip;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private SetBasedOnLanguage textBasedOnLanguage;

        public static Tooltip I;
        private bool _shouldShowTooltip = true;

        private Dictionary<TooltipType, (string english, string hindi)> translations;
        private HashSet<TooltipType> shownTooltips;
        [SerializeField] Image SeparateHindiForHindi;

        private void Awake()
        {
            I = this;
            InitializeTranslations();
            shownTooltips = new HashSet<TooltipType>();
            if (GameData.language == Language.Hindi)
            {
                tooltipText.gameObject.AddComponent<ChangeTextMeshHindi>();
            }
        }

        private void Start()
        {
            HideTooltip();
        }

        private void Update()
        {

            //SeparateHindiForHindi.enabled = tooltipText.text.Contains("पिछली चाल को रद्द करने के");
            //if (tooltipText.text.Contains("पिछली चाल को रद्द करने के")) SeparateHindiForHindi.enabled = true;
        }

        private void InitializeTranslations()
        {
            translations = new Dictionary<TooltipType, (string english, string hindi)>
            {
                {
                    TooltipType.StartGame,
                    ("Place a tile on any available space next to an existing tile to start building your Clean City.",
                        "अपने इकोनगरी का निर्माण शुरू करने के लिए किसी भी उपलब्ध जगह पर एक टाइल रखें।")
                },

                {
                    TooltipType.HoverOverTile,
                    (" Where you place a tile affects your city’s GDP and AQI differently. \n" +
                     "GDP is a measure of how prosperous your city is. In this game, the GDP is your score. \n" +
                     "AQI is a measure of how polluted your city is. If your AQI reaches 100, the game is over.",
                        "टाइल लगाने की जगह से आपके शहर की GDP और AQI पर अलग-अलग असर पड़ता है। \n" +
                        "GDP आपके शहर की समृद्धी मापता है। इस गेम में GDP आपका हाई स्कोर है। \n" +
                        "AQI आपके शहर का प्रदुषण मापता है। आपका AQI 100 तक पहुँच गया, तो गेम ओवर हो जायेगा।"
                    )
                },
                {
                    TooltipType.Rotate,
                    ("Right-click on a tile to rotate it.",
                        "टाइल को घुमाने के लिए उस पर राइट क्लिक करें।")
                },
                {
                    TooltipType.ResidentialBuilding,
                    ("This is a house. Your residents are negatively affected by AQI and will not be able to stay in your city if it gets too high.",
                        "यह एक घर है। आपके शहर का AQI अगर बहुत अधिक हो जायेगा तो नागरिक यहाँ रह नहीं पाएंगे।")
                },

                {
                    TooltipType.AgriculturalBuilding,
                    ("This is a farm. Agriculture adds to prosperity without adding to air pollution.",
                        "यह एक खेत है. खेत वायु प्रदूषण बढ़ाए बिना समृद्धि बढ़ाती है।")
                },

                {
                    TooltipType.CommercialBuilding,
                    ("This is a shop. Commerce is the backbone of any city’s economy, but can also be a high source of air pollution.",
                        "यह एक दुकान है. व्यवसाय हर शहर की अर्थव्यवस्था की नीव होती हैं, लेकिन वायु प्रदूषण का एक उच्च स्रोत भी हो सकती हैं।")
                },

                {
                    TooltipType.IndustrialBuilding,
                    ("This is a factory. Industrial activities are the biggest source of air pollution, but they also greatly contribute to economic prosperity.",
                        "ये एक फैक्ट्री है. फैक्ट्रियां और उद्योग वायु प्रदूषण का सबसे बड़ा स्रोत हैं, लेकिन वे इकॉनमी में सबसे बड़ा योगदान देती हैं।")
                },

                {
                    TooltipType.MunicipalBuilding,
                    ("This is a government office. Urban planning keeps cities efficient and sustainable.",
                        "यह एक सरकारी कार्यालय है. शहरी नियोजन शहरों को फलोत्पादक और संधारणीय बनाए रखता है।")
                },

                {
                    TooltipType.ResearchBuilding,
                    ("This is a research facility. Around the world, new technologies are constantly being developed that are more efficient and  use renewable sources of energy, thus reducing pollution.",
                        "यह एक रिसर्च संस्था है. दुनिया भर में नई तकनीकें जाँची जा रही हैं जो ज़्यादा फलोत्पादक हैं और ऊर्जा के नवीकरणीय स्रोतों का उपयोग करती हैं, जिससे प्रदूषण कम होता है।")
                },

                {
                    TooltipType.SolutionBar,
                    ("This is a metro line. These have 6 segments which can only be placed over existing roads and must be next to each other, one by one.",
                        "यह एक मेट्रो लाइन है. इसके 6 खंड हैं जिन्हें मौजूदा सड़कों पर ही रखा जा सकता है, और उन्हें एक-एक करके एक-दूसरे के बगल में ही रखा जा सकता है।")
                },

                {
                    TooltipType.SolutionBlock,
                    ("This is a solar farm. Renewable sources of energy are the best solution we have to protect the environment in the long run.",
                        "यह एक सोलर फार्म है. लंबे समय तक पर्यावरण की रक्षा करने के लिए ऊर्जा के नवीकरणीय स्रोत सबसे अच्छा समाधान हैं।")
                },


                {
                    TooltipType.WasteBar,
                    ("This is a traffic jam. These have 4 segments which can only be placed over existing roads and must be next to each other, one by one.",
                        "यह ट्रैफिक जाम है. इसके 4 खंड हैं जिन्हें मौजूदा सड़कों पर ही रखा जा सकता है, और उन्हें एक-एक करके एक-दूसरे के बगल में ही रखा जा सकता है।")
                },

                {
                    TooltipType.WasteBlock,
                    ("This is a landfill. Non Recyclable waste and burning waste is a major cause of pollution to the environment.",
                        "यह एक लैंडफिल है. गैर-पुनर्चक्रण कचरा और कचरे का जलाना पर्यावरण प्रदूषण का एक प्रमुख कारण है।")
                },

                {
                    TooltipType.NatureBuilding,
                    ("This is a park. Green belts and parks in cities reduce air pollution.",
                        "यह एक पार्क है. शहरों में ग्रीन बेल्ट और पार्क वायु प्रदूषण को कम करते हैं।")
                },

                {
                    TooltipType.UniqueBuilding,
                    ("This is a public space. Unique places where people gather are what make each city unique and make people enjoy living there.",
                        "यह एक सार्वजनिक स्थान है. हर शहर में ऐसे विशेष स्थान होते हैं जो उसे अनोखा बनाते हैं और लोगों को वहां रहने का आनंद देते हैं।")
                },

                {
                    TooltipType.DifferentTile,
                    ("Placing a different type of tile next to one or a group of the same type of tile gives your city an extra +1 GDP",
                        "एक ही प्रकार की टाइलों के बगल में हर अलग प्रकार की टाइल लगाने से आपको 1 GDP मिलता है।")
                },

                {
                    TooltipType.SameTile,
                    ("Placing three of the same kind of tile next to another can be used to build blocks, for a boost to your GDP and AQI both.\n" +
                     "Blocks show that your city is prospering. Blocks unlock advanced types of tiles.",
                        "एक ही तरह की तीन टाइलें एक-दूसरे के बगल में रखने से वह एकजुट होकर ब्लॉक बन जाते हैं। इससे आपकी GDP और AQI दोनों को बढ़ावा होता है. \n" +
                        "ब्लॉक आपके शहर की समृद्ध दर्शाते हैं। ब्लॉक नए प्रकार की टाइलें अनलॉक करते हैं।")
                },

                {
                    TooltipType.Undo,
                    ("You can undo your last move. <sprite name=\"undoIcon\">",
                        "पिछली चाल को रद्द करने के लिए यहाँ क्लिक करें")
                },

                {
                    TooltipType.Stacking,
                    ("Certain blocks can have more tiles of the same kind stacked on top of it, for a bigger GDP boost. \n" +
                     "Stacking a block to its final level unlocks a massive GDP boost at no AQI cost - but a block can only reach a full level if certain conditions are met.",
                        "GDP को अधिक बढ़ावा देने के लिए कुछ ब्लॉकों के ऊपर एक ही तरह की अधिक टाइलें लगाई जा सकती हैं। \n" +
                        "किसी ब्लॉक को उसके अंतिम स्तर तक स्टैक करने से बिना AQI लागत के बहुत सारा GDP मिलता है - लेकिन एक ब्लॉक केवल तभी पूर्ण स्तर तक पहुंच सकता है जब कुछ शर्तें पूरी होती हैं।")
                },

                {
                    TooltipType.NoStackingResidential,
                    ("This stack cannot be completed because it is next to another fully stacked block.",
                        "यह स्टैक पूरा नहीं किया जा सकता क्योंकि यह एक और पूरी स्टैक के बगल में है।")
                },

                {
                    TooltipType.NoStackingAgricultural,
                    ("This stack cannot be completed because it is next to a commercial / industrial tile.",
                        "यह स्टैक पूरा नहीं किया जा सकता क्योंकि यह एक दुकान या फैक्ट्री के बगल में है।")
                },

                {
                    TooltipType.NoStackingCommercial,
                    ("This stack cannot be completed because it needs at least 1 house, shop, or public space to be next to it.",
                        "यह स्टैक पूरा नहीं किया जा सकता क्योंकि इसके बगल में कम से कम 1 घर, दुकान या सार्वजनिक स्थान होना आवश्यक है।")
                },

                {
                    TooltipType.NoStackingIndustrial,
                    ("This stack cannot be completed because it is next to a house, farm, shop, or public space.",
                        "यह स्टैक पूरा नहीं किया जा सकता क्योंकि यह किसी घर, खेत, दुकान या सार्वजनिक स्थान के बगल में है।")
                },
            };
        }

        public void ShouldShowTooltip(bool shouldShow)
        {
            _shouldShowTooltip = shouldShow;
        }

        public void ShowToolTipForTile(PieceType pieceType, bool isBar)
        {
            switch (pieceType)
            {
                case PieceType.Agricultural:
                    ShowTooltip(TooltipType.AgriculturalBuilding);
                    break;
                case PieceType.Commercial:
                    ShowTooltip(TooltipType.CommercialBuilding);
                    break;
                case PieceType.Industrial:
                    ShowTooltip(TooltipType.IndustrialBuilding);
                    break;
                case PieceType.Residential:
                    ShowTooltip(TooltipType.ResidentialBuilding);
                    break;
                case PieceType.Municipal:
                    ShowTooltip(TooltipType.MunicipalBuilding);
                    break;
                case PieceType.Research:
                    ShowTooltip(TooltipType.ResearchBuilding);
                    break;
                case PieceType.Nature:
                    ShowTooltip(TooltipType.NatureBuilding);
                    break;
                case PieceType.Unique:
                    ShowTooltip(TooltipType.UniqueBuilding);
                    break;
                case PieceType.Waste:
                    if (isBar)
                    {
                        ShowTooltip(TooltipType.WasteBar);
                    }
                    else
                    {
                        ShowTooltip(TooltipType.WasteBlock);
                    }

                    break;
                case PieceType.Solution:
                    if (isBar)
                    {
                        ShowTooltip(TooltipType.SolutionBar);
                    }
                    else
                    {
                        ShowTooltip(TooltipType.SolutionBlock);
                    }

                    break;
            }
        }

        public bool ShowTooltip(TooltipType type)
        {
            if (!_shouldShowTooltip || shownTooltips.Contains(type))
            {
                return false;
            }

            shownTooltips.Add(type);
            if (type == TooltipType.Undo)
            {
                if (LocalizationSettings.SelectedLocale.Identifier.Code != "en") SeparateHindiForHindi.enabled = true;
            }
            else SeparateHindiForHindi.enabled = false;
            tooltip.SetActive(true);
            var (english, hindi) = translations[type];
            textBasedOnLanguage.SetText(english, hindi);

            return true;
        }

        public void HideTooltip()
        {
            tooltip.SetActive(false);
        }
    }

    public enum TooltipType
    {
        StartGame,
        Rotate,
        HoverOverTile,
        ResidentialBuilding,
        AgriculturalBuilding,
        CommercialBuilding,
        IndustrialBuilding,
        MunicipalBuilding,
        ResearchBuilding,
        SolutionBar,
        SolutionBlock,
        WasteBar,
        WasteBlock,
        NatureBuilding,
        UniqueBuilding,
        DifferentTile,
        SameTile,
        Undo,
        Stacking,
        NoStackingResidential,
        NoStackingAgricultural,
        NoStackingCommercial,
        NoStackingIndustrial
    }
}