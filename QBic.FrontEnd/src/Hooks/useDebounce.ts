// export const useDebounce_old = (callback, params = null, timeout = 250) => {
//   const ref = useRef(callback);
//   ref.current = callback;

//   const debouncedCallback = useMemo(() => {
//     const func = () => {
//       ref.current?.(params);
//     };

//     return debounce(func, timeout);
//   }, []);

//   return debouncedCallback;
// };

const timers = {};

export const useDebounce = (callback, name, timeout, ...params: any[]) => {
  let timer = timers[name];
  if (timer) {
    clearTimeout(timer);
    timers[name] = null;
  }

  timer = setTimeout(() => {
    callback(...params);
    clearTimeout(timer);
    timers[name] = null;
  }, timeout);

  timers[name] = timer;
};

// https://www.geeksforgeeks.org/how-to-get-the-javascript-function-parameter-names-values-dynamically/
// JavaScript program to get the function
// name/values dynamically
function getParams(func) {
  // String representation of the function code
  let str = func.toString();

  // Remove comments of the form /* ... */
  // Removing comments of the form //
  // Remove body of the function { ... }
  // removing '=>' if func is arrow function
  str = str
    .replace(/\/\*[\s\S]*?\*\//g, "")
    .replace(/\/\/(.)*/g, "")
    .replace(/{[\s\S]*}/, "")
    .replace(/=>/g, "")
    .trim();

  // Start parameter names after first '('
  let start = str.indexOf("(") + 1;

  // End parameter names is just before last ')'
  let end = str.length - 1;

  let result = str.substring(start, end).split(", ");

  let params = [];

  result.forEach((element) => {
    // Removing any default value
    element = element.replace(/=[\s\S]*/g, "").trim();

    if (element.length > 0) params.push(element);
  });

  return params;
}
