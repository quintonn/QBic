var fs = require("fs");
const path = require("path");

const wwwrootPath = "../../WebsiteTemplate.Test/wwwroot/";

const targetDir = path.resolve(__dirname, wwwrootPath);

const fileExtensionsToDelete = [".css", ".css.map", ".js", ".js.map", ".html"];

const deleteFiles = () => {
  fs.readdir(targetDir, function (err, filenames) {
    if (err) {
      onError(err);
      return;
    }
    filenames.forEach(function (filename) {
      if (fileExtensionsToDelete.some((x) => filename.endsWith(x))) {
        const pathToDelete = path.resolve(__dirname, wwwrootPath, filename);

        fs.unlink(pathToDelete, (err) => {
          if (err) {
            console.error(`Error deleting file ${filename}:`, err);
          } else {
            //console.log(`Successfully deleted file ${filename}`);
          }
        });
      }
    });
  });
};

deleteFiles();
