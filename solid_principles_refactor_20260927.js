/**
 * Provides a small dependency-inversion boundary for reusable policies.
 * @author Son Nguyen <hoangson091104@gmail.com>
 */
export const applyPolicy = (policy, input) => policy.apply(input);
