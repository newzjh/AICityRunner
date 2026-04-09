using System;
using System.Collections.Generic;
using UnityEngine;

public class CityRuntimeProfile
{
    public string CityName;
    public string[] Keywords;
    public string[] ScoreItems;
    public string[] ObstacleItems;
    public string[] LifeItems;
    public string[] SpeedItems;
    public string[] CheckInItems;
    public string[] FoodStalls;
    public string[] ArcadeSigns;
    public string[] SharedBikeSpots;
    public string[] FlowerMarkets;
    public string[] TransitStops;
    public Color PrimaryColor;
    public Color AccentColor;
}

public static class CityRuntimeContent
{
    private static readonly List<CityRuntimeProfile> Profiles = new List<CityRuntimeProfile>
    {
        new CityRuntimeProfile
        {
            CityName = "广州",
            Keywords = new[] { "广州", "广府", "北京路", "永庆坊", "沙面", "珠江", "天河", "越秀", "海珠", "荔湾" },
            ScoreItems = new[] { "早茶券", "广绣徽章", "骑楼邮票", "珠江船票" },
            ObstacleItems = new[] { "施工围栏", "雨棚货架", "外摆桌椅", "配送手推车" },
            LifeItems = new[] { "凉茶铺补给", "双皮奶", "糖水铺暖心包" },
            SpeedItems = new[] { "地铁换乘卡", "珠江夜跑能量饮", "城市骑行冲刺卡" },
            CheckInItems = new[] { "广州塔打卡章", "永庆坊明信片", "沙面合影框", "北京路盖章册" },
            FoodStalls = new[] { "肠粉摊", "艇仔粥档", "广式糖水铺" },
            ArcadeSigns = new[] { "西关骑楼招牌", "醒狮巡游牌", "非遗广绣灯牌" },
            SharedBikeSpots = new[] { "珠江绿道单车点", "骑楼街骑行点" },
            FlowerMarkets = new[] { "岭南花市", "年花小铺", "榕树盆景摊" },
            TransitStops = new[] { "APM 口", "有轨电车站", "珠江码头指引" },
            PrimaryColor = new Color(0.82f, 0.28f, 0.24f),
            AccentColor = new Color(0.98f, 0.76f, 0.28f)
        },
        new CityRuntimeProfile
        {
            CityName = "深圳",
            Keywords = new[] { "深圳", "前海", "南山", "华强北", "福田", "罗湖", "盐田", "大鹏", "深圳湾" },
            ScoreItems = new[] { "科技展徽章", "湾区创意币", "人才公园贴纸", "海岸线纪念票" },
            ObstacleItems = new[] { "快递无人车", "共享充电柜", "施工隔离桩", "夜市外摆架" },
            LifeItems = new[] { "咖啡补给", "鲜榨果汁", "深夜便利店能量包" },
            SpeedItems = new[] { "创新加速芯片", "湾区冲刺卡", "地铁极速通" },
            CheckInItems = new[] { "前海打卡章", "华强北寻宝卡", "南头古城通关贴", "深圳湾观景票" },
            FoodStalls = new[] { "创意咖啡车", "海岸轻食摊", "文创雪糕车" },
            ArcadeSigns = new[] { "霓虹科技灯牌", "创客街区导视", "未来感巨幅屏" },
            SharedBikeSpots = new[] { "湾区骑行驿站", "公园共享单车列" },
            FlowerMarkets = new[] { "公园花境", "口袋花园摊", "海风绿植角" },
            TransitStops = new[] { "地铁枢纽口", "滨海巴士站", "人才公园接驳点" },
            PrimaryColor = new Color(0.16f, 0.68f, 0.86f),
            AccentColor = new Color(0.67f, 0.35f, 0.98f)
        },
        new CityRuntimeProfile
        {
            CityName = "佛山",
            Keywords = new[] { "佛山", "祖庙", "岭南天地", "顺德", "千灯湖", "南海", "禅城" },
            ScoreItems = new[] { "醒狮徽章", "陶艺票", "功夫馆门票", "岭南天地贴纸" },
            ObstacleItems = new[] { "陶瓷木箱", "醒狮鼓架", "市集花车", "快递堆架" },
            LifeItems = new[] { "双皮奶补给", "盲公饼", "岭南凉茶" },
            SpeedItems = new[] { "龙舟冲刺令", "功夫加速符", "地铁畅行牌" },
            CheckInItems = new[] { "祖庙打卡章", "岭南天地通票", "千灯湖夜游卡", "顺峰山合影框" },
            FoodStalls = new[] { "顺德小吃摊", "鱼生档", "牛杂档" },
            ArcadeSigns = new[] { "醒狮巡游牌", "功夫街招牌", "陶艺市集灯牌" },
            SharedBikeSpots = new[] { "千灯湖骑行点", "祖庙慢行点" },
            FlowerMarkets = new[] { "盆景摊", "兰花小铺", "岭南花卉车" },
            TransitStops = new[] { "广佛线换乘口", "水巴导视牌", "古镇接驳站" },
            PrimaryColor = new Color(0.78f, 0.48f, 0.2f),
            AccentColor = new Color(0.95f, 0.84f, 0.42f)
        },
        new CityRuntimeProfile
        {
            CityName = "东莞",
            Keywords = new[] { "东莞", "松山湖", "可园", "莞城", "滨海湾", "虎门", "东城" },
            ScoreItems = new[] { "莞香徽章", "潮玩贴纸", "松山湖纪念币", "滨海慢跑章" },
            ObstacleItems = new[] { "集装箱箱笼", "潮玩展架", "露营折叠桌", "货运推车" },
            LifeItems = new[] { "糖水补给", "莞香茶包", "能量果杯" },
            SpeedItems = new[] { "潮玩冲刺卡", "湾区快线券", "骑行加速条" },
            CheckInItems = new[] { "可园打卡章", "松山湖骑行票", "虎门炮台卡", "滨海湾观景卡" },
            FoodStalls = new[] { "烧鹅濑粉摊", "糖水车", "潮玩饮品车" },
            ArcadeSigns = new[] { "潮玩街区灯牌", "莞香文化牌", "滨海夜跑导视" },
            SharedBikeSpots = new[] { "松山湖骑行站", "滨海单车列" },
            FlowerMarkets = new[] { "滨海绿植角", "花艺车", "可园盆景摊" },
            TransitStops = new[] { "城轨口", "滨海公交站", "公园接驳牌" },
            PrimaryColor = new Color(0.2f, 0.57f, 0.44f),
            AccentColor = new Color(0.96f, 0.63f, 0.24f)
        },
        new CityRuntimeProfile
        {
            CityName = "珠海",
            Keywords = new[] { "珠海", "情侣路", "横琴", "香洲", "野狸岛", "日月贝", "长隆" },
            ScoreItems = new[] { "海风贝壳章", "情侣路纪念票", "日月贝徽章", "海岛邮票" },
            ObstacleItems = new[] { "防浪沙袋", "渔箱堆", "观景围栏", "海边施工桶" },
            LifeItems = new[] { "海盐柠檬饮", "海岛能量包", "鲜果杯" },
            SpeedItems = new[] { "横琴快线卡", "海岸冲浪板", "滨海骑行令" },
            CheckInItems = new[] { "日月贝打卡章", "情侣路明信片", "长隆入园票", "港珠澳观景章" },
            FoodStalls = new[] { "海鲜轻食摊", "椰子车", "海风冰饮摊" },
            ArcadeSigns = new[] { "滨海观景牌", "海岛霓虹牌", "渔港灯牌" },
            SharedBikeSpots = new[] { "情侣路单车站", "海岸骑行列" },
            FlowerMarkets = new[] { "三角梅花境", "海风绿植摊", "滨海花车" },
            TransitStops = new[] { "港珠澳接驳点", "海岸巴士站", "横琴口岸导视" },
            PrimaryColor = new Color(0.18f, 0.63f, 0.83f),
            AccentColor = new Color(1f, 0.84f, 0.45f)
        },
        new CityRuntimeProfile
        {
            CityName = "中山",
            Keywords = new[] { "中山", "孙文西", "岐江", "詹园", "小榄", "石岐" },
            ScoreItems = new[] { "岐江夜游票", "香山徽章", "孙文西贴纸", "步行街纪念币" },
            ObstacleItems = new[] { "市集木架", "骑楼货箱", "施工路障", "花车" },
            LifeItems = new[] { "杏仁饼", "凉茶补给", "鲜花饼" },
            SpeedItems = new[] { "岐江冲刺卡", "步行街快行券", "轻轨加速牌" },
            CheckInItems = new[] { "孙文西打卡章", "岐江河畔票", "詹园合影框", "香山古城卡" },
            FoodStalls = new[] { "杏仁饼摊", "艇仔粥档", "糖水小车" },
            ArcadeSigns = new[] { "香山骑楼招牌", "步行街灯牌", "侨乡导览牌" },
            SharedBikeSpots = new[] { "岐江骑行点", "古城单车列" },
            FlowerMarkets = new[] { "花木摊", "盆景铺", "园林花车" },
            TransitStops = new[] { "轻轨站口", "岐江码头牌", "古城接驳点" },
            PrimaryColor = new Color(0.62f, 0.34f, 0.24f),
            AccentColor = new Color(0.94f, 0.78f, 0.31f)
        },
        new CityRuntimeProfile
        {
            CityName = "惠州",
            Keywords = new[] { "惠州", "西湖", "巽寮湾", "罗浮山", "惠东", "惠阳" },
            ScoreItems = new[] { "西湖船票", "山海徽章", "罗浮山福签", "湾畔纪念贴" },
            ObstacleItems = new[] { "渔网箱", "路边围栏", "旅行箱堆", "摊位货架" },
            LifeItems = new[] { "梅菜饼", "山泉补给", "海盐果茶" },
            SpeedItems = new[] { "山海冲刺卡", "沿湖快步券", "滨海滑板牌" },
            CheckInItems = new[] { "西湖打卡章", "罗浮山纪念卡", "巽寮湾明信片", "祝屋巷盖章册" },
            FoodStalls = new[] { "梅菜饼摊", "海风饮品车", "手工糖水档" },
            ArcadeSigns = new[] { "西湖夜游牌", "山海导视牌", "古巷灯牌" },
            SharedBikeSpots = new[] { "西湖单车点", "沿湖骑行站" },
            FlowerMarkets = new[] { "湖畔花摊", "山野绿植角", "盆景车" },
            TransitStops = new[] { "景区接驳点", "滨海巴士站", "步道导视牌" },
            PrimaryColor = new Color(0.24f, 0.58f, 0.44f),
            AccentColor = new Color(0.94f, 0.71f, 0.32f)
        },
        new CityRuntimeProfile
        {
            CityName = "江门",
            Keywords = new[] { "江门", "开平", "赤坎", "启明里", "侨都", "台山", "新会" },
            ScoreItems = new[] { "侨乡明信片", "碉楼门票", "启明里徽章", "小鸟天堂贴纸" },
            ObstacleItems = new[] { "木箱堆", "旧街围栏", "露天座椅", "货摊架" },
            LifeItems = new[] { "陈皮茶", "古镇糖水", "侨乡糕点" },
            SpeedItems = new[] { "侨乡快线卡", "古镇冲刺牌", "骑楼疾行券" },
            CheckInItems = new[] { "赤坎古镇打卡章", "开平碉楼门票", "启明里合影框", "侨都通关卡" },
            FoodStalls = new[] { "陈皮茶铺", "牛杂摊", "古镇糖水档" },
            ArcadeSigns = new[] { "侨乡骑楼招牌", "古镇灯牌", "电影感街区牌" },
            SharedBikeSpots = new[] { "侨乡骑行点", "古镇单车列" },
            FlowerMarkets = new[] { "骑楼花摊", "陈皮绿植角", "侨乡花车" },
            TransitStops = new[] { "古镇接驳点", "侨都巴士站", "观景导视牌" },
            PrimaryColor = new Color(0.72f, 0.4f, 0.24f),
            AccentColor = new Color(0.96f, 0.82f, 0.5f)
        },
        new CityRuntimeProfile
        {
            CityName = "肇庆",
            Keywords = new[] { "肇庆", "七星岩", "鼎湖山", "端州", "牌坊", "宋城" },
            ScoreItems = new[] { "端砚徽章", "七星岩票根", "鼎湖山贴纸", "星湖纪念章" },
            ObstacleItems = new[] { "景区围栏", "石景护栏", "货箱堆", "路障桶" },
            LifeItems = new[] { "裹蒸粽", "山泉补给", "糖水包" },
            SpeedItems = new[] { "星湖冲刺卡", "登山疾行券", "绿道加速牌" },
            CheckInItems = new[] { "七星岩打卡章", "鼎湖山纪念卡", "牌坊广场章", "端州游览票" },
            FoodStalls = new[] { "裹蒸粽摊", "山泉饮品车", "端砚文创摊" },
            ArcadeSigns = new[] { "山湖导视牌", "宋城骑楼灯牌", "景区夜游牌" },
            SharedBikeSpots = new[] { "星湖单车站", "绿道骑行点" },
            FlowerMarkets = new[] { "山水花摊", "岭南盆景角", "景区花车" },
            TransitStops = new[] { "景区接驳站", "牌坊广场口", "绿道驿站" },
            PrimaryColor = new Color(0.28f, 0.52f, 0.36f),
            AccentColor = new Color(0.9f, 0.78f, 0.4f)
        },
        new CityRuntimeProfile
        {
            CityName = "香港",
            Keywords = new[] { "香港", "尖沙咀", "中环", "维港", "西九", "铜锣湾", "叮叮车", "旺角", "庙街" },
            ScoreItems = new[] { "维港船票", "叮叮车票", "中环徽章", "霓虹街区贴纸" },
            ObstacleItems = new[] { "路边路锥", "霓虹招牌架", "行李箱堆", "外卖手推车" },
            LifeItems = new[] { "港式奶茶", "菠萝包", "鱼蛋能量包" },
            SpeedItems = new[] { "八达通冲刺卡", "天星码头快线票", "山顶疾行券" },
            CheckInItems = new[] { "维港打卡章", "天星小轮船票", "西九文化票", "庙街夜游卡" },
            FoodStalls = new[] { "鸡蛋仔摊", "鱼蛋车", "港式奶茶档" },
            ArcadeSigns = new[] { "霓虹灯牌", "叮叮车导视牌", "海港巨幅招牌" },
            SharedBikeSpots = new[] { "海滨骑行点", "西九单车列" },
            FlowerMarkets = new[] { "花墟摊位", "口袋花店", "海港绿植角" },
            TransitStops = new[] { "叮叮车站", "天星码头指引", "港铁口" },
            PrimaryColor = new Color(0.92f, 0.22f, 0.25f),
            AccentColor = new Color(0.93f, 0.82f, 0.35f)
        },
        new CityRuntimeProfile
        {
            CityName = "澳门",
            Keywords = new[] { "澳门", "大三巴", "官也街", "新马路", "路环", "氹仔", "金莲花" },
            ScoreItems = new[] { "葡式瓷砖章", "大三巴票根", "官也街徽章", "路环纪念贴" },
            ObstacleItems = new[] { "路边花架", "露台餐桌", "行李车", "施工围挡" },
            LifeItems = new[] { "葡挞", "杏仁饼", "柠檬茶补给" },
            SpeedItems = new[] { "石板路冲刺卡", "轻轨快线券", "观景步道加速牌" },
            CheckInItems = new[] { "大三巴打卡章", "官也街明信片", "新马路盖章册", "路环渔村合影框" },
            FoodStalls = new[] { "葡挞摊", "猪扒包车", "手信铺外摆" },
            ArcadeSigns = new[] { "葡式街牌", "节庆灯牌", "石板路导视牌" },
            SharedBikeSpots = new[] { "海滨慢行点", "轻轨接驳单车位" },
            FlowerMarkets = new[] { "欧式花篮摊", "节庆花车", "街角绿植铺" },
            TransitStops = new[] { "轻轨站口", "大三巴导视牌", "海滨接驳点" },
            PrimaryColor = new Color(0.4f, 0.28f, 0.58f),
            AccentColor = new Color(0.98f, 0.76f, 0.45f)
        }
    };

    public static readonly DynamicStreetItemType[] ExtraSceneryTypes =
    {
        DynamicStreetItemType.FoodStall,
        DynamicStreetItemType.ArcadeSign,
        DynamicStreetItemType.SharedBikeSpot,
        DynamicStreetItemType.FlowerMarket,
        DynamicStreetItemType.TransitStop
    };

    public static CityRuntimeProfile ResolveProfile(string street)
    {
        if (!string.IsNullOrWhiteSpace(street))
        {
            foreach (CityRuntimeProfile profile in Profiles)
            {
                foreach (string keyword in profile.Keywords)
                {
                    if (street.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return profile;
                    }
                }
            }
        }

        return Profiles[0];
    }

    public static string ResolveCityName(string street)
    {
        return ResolveProfile(street).CityName;
    }

    public static List<CityRuntimeProfile> GetAllProfiles()
    {
        return new List<CityRuntimeProfile>(Profiles);
    }

    public static string PickLabel(CityRuntimeProfile profile, DynamicStreetItemType itemType)
    {
        switch (itemType)
        {
            case DynamicStreetItemType.ScorePickup:
                return PickRandom(profile.ScoreItems);
            case DynamicStreetItemType.Obstacle:
                return PickRandom(profile.ObstacleItems);
            case DynamicStreetItemType.LifePickup:
                return PickRandom(profile.LifeItems);
            case DynamicStreetItemType.SpeedPickup:
                return PickRandom(profile.SpeedItems);
            case DynamicStreetItemType.CheckInPickup:
                return PickRandom(profile.CheckInItems);
            case DynamicStreetItemType.FoodStall:
                return PickRandom(profile.FoodStalls);
            case DynamicStreetItemType.ArcadeSign:
                return PickRandom(profile.ArcadeSigns);
            case DynamicStreetItemType.SharedBikeSpot:
                return PickRandom(profile.SharedBikeSpots);
            case DynamicStreetItemType.FlowerMarket:
                return PickRandom(profile.FlowerMarkets);
            case DynamicStreetItemType.TransitStop:
                return PickRandom(profile.TransitStops);
            default:
                return profile.CityName;
        }
    }

    private static string PickRandom(string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return string.Empty;
        }

        return values[UnityEngine.Random.Range(0, values.Length)];
    }
}
