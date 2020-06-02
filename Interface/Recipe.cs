using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    public class Recipe
    {
        /// <summary>
        /// 程序名称
        /// </summary>
        public string RecipeName { get; set; }

        /// <summary>
        /// 程序版本
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 程序路径
        /// </summary>
        public string FilePath { get; set; }
    }
}
