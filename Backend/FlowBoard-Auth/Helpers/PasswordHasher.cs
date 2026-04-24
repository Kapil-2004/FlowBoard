namespace FlowBoard.Auth.Helpers
{
    /// <summary>
    /// Wrapper around BCrypt.Net so that all hashing logic is in one place.
    /// Using work factor 12 – strong enough for production, fast enough for tests.
    /// </summary>
    public static class PasswordHasher
    {
        private const int WorkFactor = 12;

        /// <summary>
        /// Hashes a plain-text password using BCrypt.
        /// The resulting hash is safe to store in the database.
        /// </summary>
        public static string Hash(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(plainTextPassword));

            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
        }

        /// <summary>
        /// Verifies a plain-text password against a stored BCrypt hash.
        /// Returns false (not an exception) on mismatch to avoid timing attacks.
        /// </summary>
        public static bool Verify(string plainTextPassword, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword) ||
                string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }
    }
}
