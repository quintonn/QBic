export const API_VERSION = "v1";

const scheme = window.location.protocol;
let _url = scheme + "//" + window.location.host + window.location.pathname;

if (process.env.ROOT_URL) {
  _url = process.env.ROOT_URL;
}

if (!_url.endsWith("/")) {
  _url += "/";
}

export const BASE_URL = _url;

export const API_URL = `${_url}api/${API_VERSION}/`;
