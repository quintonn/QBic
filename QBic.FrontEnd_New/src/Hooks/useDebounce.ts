import { useMemo, useRef } from "react";
const debounce = require("lodash/debounce");

export const useDebounce = (callback) => {
  const ref = useRef(callback);
  ref.current = callback;

  const debouncedCallback = useMemo(() => {
    const func = () => {
      ref.current?.();
    };

    return debounce(func, 250);
  }, []);

  return debouncedCallback;
};
