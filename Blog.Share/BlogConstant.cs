namespace Blog.Share
{
    public static class BlogConstant
    {
        public static class RedisKey
        {
            public const string CACHE_KEY_BLOG = "CACHE_KEY_BLOG";
            public const string CACHE_KEY_BLOG_CATEGORY = "CACHE_KEY_BLOG_CATEGORY";
            public const string CACHE_KEY_BLOG_CATEGORY_MAP = "CACHE_KEY_BLOG_CATEGORY_MAP";
            public const string CACHE_KEY_BLOG_TAG_MAP = "CACHE_KEY_BLOG_TAG_MAP";
            public const string CACHE_KEY_BLOG_TAG = "CACHE_KEY_BLOG_TAG";
        }

        public static class CommonMessageBlog
        {
            public const string REGISTER_BLOG_SUCCESSFUL = "Thêm bài viết thành công";
            public const string UPDATE_PROFILE_SUCCESS = "Cập nhật thông tin bài viết thành công";
            public const string SYNC_BLOG_SUCCESS = "SYNC_BLOG_SUCCESS";
            public const string SYNC_BLOG_CATEGORY_SUCCESS = "SYNC_BLOG_CATEGORY_SUCCESS";
            public const string SYNC_BLOG_TAG_SUCCESS = "SYNC_BLOG_TAG_SUCCESS";
        }

        public static class Message
        {
            public static string CATEGORY_DO_NOT_EXIST = "Category do not exist";
            public static string TAG_DO_NOT_EXIST = "Tag do not exist";
            public static string SUBMIT_SUCCESSFULLY = "Submitted successfully";
            public static string RECORD_DO_NOT_EXIST = "Record does not exist";
        }

        public static class SiteSettings
        {
            public static class Keys
            {
                public const string ADS_BANNER = "ADS_BANNER";
                
                public const string ADS_MANAGEMENT = "ADS_MANAGEMENT";
            }
        }
    }
}