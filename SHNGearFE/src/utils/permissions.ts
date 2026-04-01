export function hasAdminAccess(permissions: string[] = []): boolean {
  return permissions.some((permission) =>
    permission.startsWith('account.') ||
    permission.startsWith('role.') ||
    permission.startsWith('permission.') ||
    permission.startsWith('product.') ||
    permission.startsWith('brand.') ||
    permission.startsWith('category.')
  );
}
