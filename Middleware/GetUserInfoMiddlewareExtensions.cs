namespace Gateway.Middleware
{
    public static class GetUserInfoMiddlewareExtensions
    {
        public static IApplicationBuilder UseGetUserInfo(this
            IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GetUserInfoMiddleware>();
        }
    }
}
