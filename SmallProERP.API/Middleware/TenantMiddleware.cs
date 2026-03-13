namespace SmallProERP.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantIdClaim = context.User.FindFirst("TenantId");

                if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
                {
                    context.Items["TenantId"] = tenantId;
                }
            }

            await _next(context);
        }
    }
}