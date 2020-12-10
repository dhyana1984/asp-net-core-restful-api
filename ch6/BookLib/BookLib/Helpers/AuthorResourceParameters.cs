using System;
namespace BookLib.Helpers
{
    public class AuthorResourceParameters
    {
        //每一页最大数据条数
        public const int MaxPageSize = 50;
        private int _pagesize = 10;

        //页码
        public int PageNumber { get; set; } = 1;
        //每页数据条数
        public int PageSize
        {
            get { return _pagesize; }
            set
            {
                _pagesize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }
    }
}
