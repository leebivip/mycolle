
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

static class StrDealer
{
    const int infinity = 999999;

    /// <summary>
    /// 判断两个字符串的相似度, 1.0 为完全相同
    /// <summary>
    public static decimal Similarity(string lhs, string rhs)
    {
        var strc = new StringCompute();
        strc.Compute(lhs, rhs);
        return strc.ComputeResult.Rate;
    }

    /// <summary>
    /// 获取一个字符串的所有子集
    /// </summary>
    public static string[] SubSets(string str)
    {
        return _subset_inter(str)
            // 分组去重
            .GroupBy(x => x)
            .Select(x => x.Key)
            // 排序
            .OrderByDescending(x => x)
            .OrderByDescending(x => x.Length)
            // 转为数组
            .ToArray();
    }


    /// <summary>
    /// 使用递归获取一个字符串的所有子集, 包括重复内容
    /// </summary>
    static List<string> _subset_inter(string str)
    {
        var ret = new List<string>();

        for (int i = 0; i < str.Length; ++i)
        {
            if (str.Length == 1) break;

            var next = str.Remove(i, 1);
            ret.Add(next);

            ret.AddRange(_subset_inter(next));
        }

        return ret;
    }

    /// <summary>
    /// 获取一个字符串的所有子串
    /// </summary>
    /// <param name="str">要分割的字符串</param>
    /// <param name="depth">长度. 数字越大, 长度越短, 0为最长</param>
    /// <returns>分割结果</returns>
    public static string[] SubStrs(string str, int depth = infinity)
    {
        var ret = new List<string>();

        for (int i = 0; i < str.Length && i <= depth; ++i)
        {
            for (int j = 0; j <= i; ++j)
            {
                ret.Add(str.Substring(j, str.Length - i));
            }
        }

        return ret.GroupBy(x => x).Select(x => x.Key).ToArray();
    }

    // 以下内容出处: http://www.cnblogs.com/stone_w/archive/2012/08/16/2642679.html

    /// <summary>
    /// 相似度检测器
    /// </summary>
    class StringCompute
    {
        #region 私有变量

        /// <summary>
        /// 字符串1
        /// </summary>
        private char[] _ArrChar1;
        /// <summary>
        /// 字符串2
        /// </summary>
        private char[] _ArrChar2;
        /// <summary>
        /// 统计结果
        /// </summary>
        private Result _Result;
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _BeginTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _EndTime;
        /// <summary>
        /// 计算次数
        /// </summary>
        private int _ComputeTimes;
        /// <summary>
        /// 算法矩阵
        /// </summary>
        private int[,] _Matrix;
        /// <summary>
        /// 矩阵列数
        /// </summary>
        private int _Column;
        /// <summary>
        /// 矩阵行数
        /// </summary>
        private int _Row;
        #endregion

        #region 属性
        /// <summary>
        /// 计算结果
        /// </summary>
        public struct Result
        {
            /// <summary>
            /// 相似度
            /// </summary>
            public decimal Rate;
            /// <summary>
            /// 对比次数
            /// </summary>
            public string ComputeTimes;
            /// <summary>
            /// 使用时间
            /// </summary>
            public string UseTime;
            /// <summary>
            /// 差异
            /// </summary>
            public int Difference;
        }
        public Result ComputeResult
        { get { return _Result; } }
        #endregion

        #region 构造函数
        public StringCompute(string str1, string str2)
        { this.StringComputeInit(str1, str2); }
        public StringCompute() { }
        #endregion

        #region 算法实现
        /// <summary>
        /// 初始化算法基本信息
        /// </summary>
        private void StringComputeInit(string str1, string str2)
        {
            _ArrChar1 = str1.ToCharArray();
            _ArrChar2 = str2.ToCharArray();
            _Result = new Result();
            _ComputeTimes = 0;
            _Row = _ArrChar1.Length + 1;
            _Column = _ArrChar2.Length + 1;
            _Matrix = new int[_Row, _Column];
        }
        /// <summary>
        /// 计算相似度
        /// </summary>
        public void Compute()
        {
            //开始时间
            _BeginTime = DateTime.Now;
            //初始化矩阵的第一行和第一列
            this.InitMatrix();
            int intCost = 0;
            for (int i = 1; i < _Row; i++)
            {
                for (int j = 1; j < _Column; j++)
                {
                    if (_ArrChar1[i - 1] == _ArrChar2[j - 1])
                    { intCost = 0; }
                    else
                    { intCost = 1; }
                    //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                    //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                    _Matrix[i, j] = this.Minimum(_Matrix[i - 1, j] + 1, _Matrix[i, j - 1] + 1, _Matrix[i - 1, j - 1] + intCost);
                    _ComputeTimes++;
                }
            }
            //结束时间
            _EndTime = DateTime.Now;
            //相似率 移动次数小于最长的字符串长度的20%算同一题
            int intLength = _Row > _Column ? _Row : _Column;

            _Result.Rate = (1 - (decimal)_Matrix[_Row - 1, _Column - 1] / intLength);
            _Result.UseTime = (_EndTime - _BeginTime).ToString();
            _Result.ComputeTimes = _ComputeTimes.ToString();
            _Result.Difference = _Matrix[_Row - 1, _Column - 1];
        }

        /// <summary>
        /// 计算相似度
        /// </summary>
        public void Compute(string str1, string str2)
        {
            this.StringComputeInit(str1, str2);
            this.Compute();
        }

        /// <summary>
        /// 初始化矩阵的第一行和第一列
        /// </summary>
        private void InitMatrix()
        {
            for (int i = 0; i < _Column; i++)
            { _Matrix[0, i] = i; }
            for (int i = 0; i < _Row; i++)
            { _Matrix[i, 0] = i; }
        }
        /// <summary>
        /// 取三个数中的最小值
        /// </summary>
        private int Minimum(int First, int Second, int Third)
        {
            int intMin = First;
            if (Second < intMin)
            { intMin = Second; }
            if (Third < intMin)
            { intMin = Third; }
            return intMin;
        }
        #endregion
    }
}
