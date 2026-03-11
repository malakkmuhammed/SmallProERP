namespace SmallProERP.API.Helpers
{
    public static class TestHelper
    {
        // Toggle this for testing vs production
        public static bool IsTestMode => true;  // ⭐ Set to false when going to production

        public static int TestTenantId => 1;
        public static int TestUserId => 1;
    }
}
