
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 简单的模糊搜索工具
/// </summary>
static class FuzzyFind
{
    const int infinity = 999999;

    /// <summary>
    /// 对一组字符串执行模糊搜索
    /// </summary>
    /// <param name="lhs">要查询的字符串数组</param>
    /// <param name="rhs">关键字</param>
    /// <param name="depth">精确度. 数字越小, 精度越高</param>
    /// <returns>搜索结果的字符串数组</returns>
    public static string[] Find(string[] ori, string match, int depth = infinity)
    {
        var ret = new List<string>();
        var subs = StrDealer.SubStrs(match, depth);

        foreach (var s in subs)
        { ret.AddRange(ori.Where(x => x.Contains(s))); }

        return ret.GroupBy(x => x).Select(x => x.Key).ToArray();
    }

    /// <summary>
    /// 对一组字符串执行模糊搜索, 空格分离多关键字
    /// 将从上一个关键字的结果中搜索下一个关键字
    /// </summary>
    /// <param name="lhs">要查询的字符串数组</param>
    /// <param name="rhs">关键字, 用空格分离</param>
    /// <param name="depth">精确度. 数字越小, 精度越高</param>
    /// <returns>搜索结果的字符串数组</returns>
    public static string[] MultiFind(string[] ori, string match, int depth = infinity)
    {
        //用空格分离关键字
        var multi = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var single in multi)
        { ori = Find(ori, single, depth); }

        return ori.GroupBy(x => x).Select(x => x.Key).ToArray();
    }
}
