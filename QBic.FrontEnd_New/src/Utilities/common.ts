/**
 * Utility function to get a value from an object at given a path.
 */
export function getValueAtPath(obj: any, path: string): any {
  return path.split(".").reduce((current, key) => {
    if (
      current === undefined ||
      current === null ||
      typeof current !== "object"
    ) {
      return undefined;
    }

    if (Array.isArray(current)) {
      return current.map((item) => item[key]);
    } else {
      return current?.[key];
    }
  }, obj);
}
