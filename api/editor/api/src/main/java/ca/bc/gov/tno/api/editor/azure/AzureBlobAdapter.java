package ca.bc.gov.tno.api.editor.azure;

import com.azure.storage.blob.BlobClient;
import com.azure.storage.blob.BlobContainerClient;
import com.azure.storage.blob.models.BlobProperties;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;

@Component("azureBlobAdapter")
@Service
public class AzureBlobAdapter {

  @Autowired
  BlobContainerClient containerClient;

  public String upload(MultipartFile file, String prefixName) throws IOException {
    if (file != null && file.getSize() > 0) {
      try {
        // String fileName = prefixName + UUID.randomUUID().toString() +
        // file.getOriginalFilename();
        String fileName = file.getOriginalFilename();
        BlobClient blobClient = containerClient.getBlobClient(fileName);
        blobClient.upload(file.getInputStream(), file.getSize());
        return fileName;
      } catch (IOException e) {
        e.printStackTrace();
        throw e;
      }
    }
    return null;
  }

  public byte[] getFile(String name) throws IOException {
    try {
      File temp = new File(name);
      BlobClient blobClient = containerClient.getBlobClient(name);
      BlobProperties properties = blobClient.downloadToFile(temp.getPath());
      byte[] content = Files.readAllBytes(Paths.get(temp.getPath()));
      temp.delete();
      return content;
    } catch (Exception e) {
      e.printStackTrace();
      throw e;
    }
  }

  public boolean deleteFile(String name) {
    try {
      BlobClient blobClient = containerClient.getBlobClient(name);
      blobClient.delete();
      return true;
    } catch (Exception e) {
      e.printStackTrace();
      return false;
    }

  }

}
