package com.example.codeai.Controllers;

import lombok.AllArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.stream.Stream;
import java.util.concurrent.atomic.AtomicInteger;
import org.kohsuke.github.GitHub;
import org.kohsuke.github.GitHubBuilder;
import org.kohsuke.github.GHRepository;
import org.kohsuke.github.GHContent;
import org.kohsuke.github.GHCommitBuilder;
import java.util.Base64;
import com.example.codeai.Repositories.IGitTokensRepository;

@RestController
@RequestMapping("/local")
public class LocalFileController {

    private final IGitTokensRepository gitTokensRepository;
    private static final String BASE_STORAGE_PATH = System.getProperty("user.home") + "/CodeAI_localhost";
    private static final String FLOW_FILE = "Flow.txt";
    private static final String CODE_FILE = "Code.txt";

    public LocalFileController(IGitTokensRepository gitTokensRepository) {
        this.gitTokensRepository = gitTokensRepository;
    }

    @PostMapping("/save/flow")
    public ResponseEntity<String> saveFlowContent(@RequestBody SaveContentRequest request) {
        try {
            // Save to user's CodeAI_Root directory
            String sanitizedProjectName = sanitizeFileName(request.getProjectName());
            Path projectPath = Paths.get(BASE_STORAGE_PATH, sanitizedProjectName);
            
            // Create project directory if it doesn't exist
            if (!Files.exists(projectPath)) {
                Files.createDirectories(projectPath);
            }
            
            // Create Flow.txt file path
            Path flowFilePath = projectPath.resolve(FLOW_FILE);
            
            // Save the content to file
            Files.write(flowFilePath, request.getContent().getBytes());
            
            return ResponseEntity.ok("Flow content saved successfully to: " + flowFilePath.toString());
        } catch (IOException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to save Flow content: " + e.getMessage());
        }
    }

    @PostMapping("/save/code")
    public ResponseEntity<String> saveCodeContent(@RequestBody SaveContentRequest request) {
        try {
            // Save to user's CodeAI_Root directory
            String sanitizedProjectName = sanitizeFileName(request.getProjectName());
            Path projectPath = Paths.get(BASE_STORAGE_PATH, sanitizedProjectName);
            
            // Create project directory if it doesn't exist
            if (!Files.exists(projectPath)) {
                Files.createDirectories(projectPath);
            }
            
            // Create Code.txt file path
            Path codeFilePath = projectPath.resolve(CODE_FILE);
            
            // Save the content to file
            Files.write(codeFilePath, request.getContent().getBytes());
            
            return ResponseEntity.ok("Code content saved successfully to: " + codeFilePath.toString());
        } catch (IOException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to save Code content: " + e.getMessage());
        }
    }

    @GetMapping("/read/flow/{projectId}")
    public ResponseEntity<String> readFlowContent(@PathVariable Integer projectId, @RequestParam String projectName) {
        try {
            // Read from user's CodeAI_Root directory
            String sanitizedProjectName = sanitizeFileName(projectName);
            Path projectPath = Paths.get(BASE_STORAGE_PATH, sanitizedProjectName);
            Path flowFilePath = projectPath.resolve(FLOW_FILE);
            
            if (!Files.exists(flowFilePath)) {
                return ResponseEntity.ok(""); // Return empty string if file doesn't exist
            }
            
            String content = Files.readString(flowFilePath);
            return ResponseEntity.ok(content);
        } catch (IOException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to read Flow content: " + e.getMessage());
        }
    }

    @GetMapping("/read/code/{projectId}")
    public ResponseEntity<String> readCodeContent(@PathVariable Integer projectId, @RequestParam String projectName) {
        try {
            // Read from user's CodeAI_Root directory
            String sanitizedProjectName = sanitizeFileName(projectName);
            Path projectPath = Paths.get(BASE_STORAGE_PATH, sanitizedProjectName);
            Path codeFilePath = projectPath.resolve(CODE_FILE);
            
            if (!Files.exists(codeFilePath)) {
                return ResponseEntity.ok(""); // Return empty string if file doesn't exist
            }
            
            String content = Files.readString(codeFilePath);
            return ResponseEntity.ok(content);
        } catch (IOException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to read Code content: " + e.getMessage());
        }
    }

    @GetMapping("/list/{projectId}")
    public ResponseEntity<ProjectFilesResponse> listProjectFiles(@PathVariable Integer projectId, @RequestParam String projectName) {
        try {
            String projectPath = createProjectDirectory(projectId, projectName);
            
            ProjectFilesResponse response = new ProjectFilesResponse();
            
            // Check Flow.txt
            Path flowFilePath = Paths.get(projectPath, FLOW_FILE);
            if (Files.exists(flowFilePath)) {
                response.setFlowExists(true);
                response.setFlowLastModified(Files.getLastModifiedTime(flowFilePath).toString());
                response.setFlowSize(Files.size(flowFilePath));
            }
            
            // Check Code.txt
            Path codeFilePath = Paths.get(projectPath, CODE_FILE);
            if (Files.exists(codeFilePath)) {
                response.setCodeExists(true);
                response.setCodeLastModified(Files.getLastModifiedTime(codeFilePath).toString());
                response.setCodeSize(Files.size(codeFilePath));
            }
            
            response.setProjectPath(projectPath);
            return ResponseEntity.ok(response);
        } catch (IOException e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body(new ProjectFilesResponse()); // Return empty response on error
        }
    }

    @PostMapping("/pull/{projectId}")
    public ResponseEntity<String> pullProjectFiles(@PathVariable Integer projectId, @RequestParam String projectName) {
        try {
            String sanitizedProjectName = sanitizeFileName(projectName);
            
            // Create CodeAI_Root directory if it doesn't exist
            Path codeAIRootBaseDir = Paths.get(System.getProperty("user.home") + "/CodeAI_Root");
            if (!Files.exists(codeAIRootBaseDir)) {
                Files.createDirectories(codeAIRootBaseDir);
            }
            
            // Source: User's home CodeAI_localhost directory
            Path sourceDir = Paths.get(BASE_STORAGE_PATH, sanitizedProjectName);
            if (!Files.exists(sourceDir) || !Files.isDirectory(sourceDir)) {
                return ResponseEntity.status(HttpStatus.NOT_FOUND)
                    .body("Project directory not found: " + sourceDir.toString() + "\nMake sure the project exists in your CodeAI_localhost folder.");
            }
            // Target: User's home CodeAI_Root directory
            Path targetDir = Paths.get(System.getProperty("user.home") + "/CodeAI_Root").resolve(sanitizedProjectName);
            if (!Files.exists(targetDir)) {
                Files.createDirectories(targetDir);
            }
            AtomicInteger filesCopied = new AtomicInteger(0);
            try (Stream<Path> paths = Files.walk(sourceDir)) {
                paths.filter(Files::isRegularFile)
                     .forEach(sourceFile -> {
                         try {
                             Path relativePath = sourceDir.relativize(sourceFile);
                             Path targetFile = targetDir.resolve(relativePath);
                             Files.createDirectories(targetFile.getParent());
                             Files.copy(sourceFile, targetFile, StandardCopyOption.REPLACE_EXISTING);
                             filesCopied.incrementAndGet();
                         } catch (IOException e) {
                             throw new RuntimeException("Failed to copy file: " + sourceFile, e);
                         }
                     });
            }
            return ResponseEntity.ok("Project pulled successfully! Copied " + filesCopied.get() + " files from " + 
                                     sourceDir.toString() + " to " + targetDir.toString());
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to pull project: " + e.getMessage());
        }
    }

    @PostMapping("/save/folder/{projectId}")
    public ResponseEntity<String> saveProjectFolder(@PathVariable Integer projectId, @RequestParam String projectName) {
        try {
            String sanitizedProjectName = sanitizeFileName(projectName);
            
            // Create CodeAI_localhost directory if it doesn't exist
            Path localhostBaseDir = Paths.get(BASE_STORAGE_PATH);
            if (!Files.exists(localhostBaseDir)) {
                Files.createDirectories(localhostBaseDir);
            }
            
            // Source: User's home CodeAI_Root directory where user is working
            Path sourceDir = Paths.get(System.getProperty("user.home") + "/CodeAI_Root").resolve(sanitizedProjectName);
            
            // Target: User's home CodeAI_localhost directory
            Path targetDir = Paths.get(BASE_STORAGE_PATH, sanitizedProjectName);
            
            if (!Files.exists(sourceDir) || !Files.isDirectory(sourceDir)) {
                return ResponseEntity.status(HttpStatus.NOT_FOUND)
                    .body("Project directory not found in ~/CodeAI_Root folder: " + sourceDir.toString());
            }
            
            // Create target directory if it doesn't exist
            if (!Files.exists(targetDir)) {
                Files.createDirectories(targetDir);
            }
            
            // Copy all files and subdirectories recursively
            AtomicInteger filesCopied = new AtomicInteger(0);
            AtomicInteger dirsCreated = new AtomicInteger(0);
            
            try (Stream<Path> paths = Files.walk(sourceDir)) {
                paths.forEach(sourcePath -> {
                    try {
                        Path relativePath = sourceDir.relativize(sourcePath);
                        Path targetPath = targetDir.resolve(relativePath);
                        
                        if (Files.isDirectory(sourcePath)) {
                            if (!Files.exists(targetPath)) {
                                Files.createDirectory(targetPath);
                                dirsCreated.incrementAndGet();
                            }
                        } else {
                            // Copy file
                            Files.createDirectories(targetPath.getParent());
                            Files.copy(sourcePath, targetPath, StandardCopyOption.REPLACE_EXISTING);
                            filesCopied.incrementAndGet();
                        }
                    } catch (IOException e) {
                        throw new RuntimeException("Failed to copy: " + sourcePath, e);
                    }
                });
            }
            
            return ResponseEntity.ok("Project folder saved successfully! " +
                                     "Copied " + filesCopied.get() + " files and " + dirsCreated.get() + " directories " +
                                     "from " + sourceDir.toString() + " to " + targetDir.toString());
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to save project folder: " + e.getMessage());
        }
    }

    @PostMapping("/push/{projectId}")
    public ResponseEntity<String> pushProjectToGitHub(@PathVariable Integer projectId, @RequestParam String projectName, @RequestParam Integer userId) {
        try {
            String sanitizedProjectName = sanitizeFileName(projectName);
            
            // Get user's GitHub token
            var tokenOptional = gitTokensRepository.findById(userId);
            if (tokenOptional.isEmpty()) {
                return ResponseEntity.status(HttpStatus.NOT_FOUND)
                    .body("GitHub token not found for user. Please configure your GitHub token first.");
            }
            
            String githubToken = tokenOptional.get().getGit_token();
            
            // Connect to GitHub
            GitHub github = new GitHubBuilder()
                    .withOAuthToken(githubToken)
                    .build();
            
            // Get user's GitHub username
            String username = github.getMyself().getLogin();
            
            // Get the repository
            GHRepository repository;
            try {
                repository = github.getRepository(username + "/" + sanitizedProjectName);
            } catch (Exception e) {
                return ResponseEntity.status(HttpStatus.NOT_FOUND)
                    .body("GitHub repository '" + sanitizedProjectName + "' not found. Please create the repository first.");
            }
            
            // Source: User's CodeAI_Root directory where user actually works
            Path sourceDir = Paths.get(System.getProperty("user.home") + "/CodeAI_Root").resolve(sanitizedProjectName);
            if (!Files.exists(sourceDir) || !Files.isDirectory(sourceDir)) {
                return ResponseEntity.status(HttpStatus.NOT_FOUND)
                    .body("Project directory not found in ~/CodeAI_Root folder: " + sourceDir.toString() + 
                          "\nMake sure you have pulled the project first or saved your work.");
            }
            
            AtomicInteger filesPushed = new AtomicInteger(0);
            AtomicInteger filesSkipped = new AtomicInteger(0);
            
            // Walk through all files in the project directory
            try (Stream<Path> paths = Files.walk(sourceDir)) {
                paths.filter(Files::isRegularFile)
                     .forEach(sourceFile -> {
                         try {
                             // Get relative path from project directory
                             Path relativePath = sourceDir.relativize(sourceFile);
                             String repoPath = relativePath.toString().replace("\\", "/");
                             
                             // Read file content as text
                             String fileContent = Files.readString(sourceFile);
                             
                             try {
                                 // Try to get existing file
                                 GHContent existingContent = repository.getFileContent(repoPath);
                                 
                                 // Update existing file with raw text content
                                 existingContent.update(fileContent, "Update " + repoPath + " via CodeAI Tools");
                                 filesPushed.incrementAndGet();
                                 
                             } catch (Exception e) {
                                // File doesn't exist, create new one with raw text content
                                repository.createContent()
                                    .content(fileContent)
                                    .message("Add " + repoPath + " via CodeAI Tools")
                                    .path(repoPath)
                                    .commit();
                                filesPushed.incrementAndGet();
                            }
                     } catch (Exception e) {
                         filesSkipped.incrementAndGet();
                     }
                 });
            }
            
            return ResponseEntity.ok("Project pushed successfully! " +
                                     "Pushed " + filesPushed.get() + " files to GitHub repository " +
                                     username + "/" + sanitizedProjectName +
                                     (filesSkipped.get() > 0 ? " (skipped " + filesSkipped.get() + " files)" : ""));
                                     
        } catch (Exception e) {
            return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                    .body("Failed to push project to GitHub: " + e.getMessage());
        }
    }

    private String createProjectDirectory(Integer projectId, String projectName) {
        try {
            // Create base directory if it doesn't exist
            Path basePath = Paths.get(BASE_STORAGE_PATH);
            if (!Files.exists(basePath)) {
                Files.createDirectories(basePath);
            }
            
            // Create project directory with project ID and name
            String sanitizedProjectName = sanitizeFileName(projectName);
            String projectDirName = projectId + "_" + sanitizedProjectName;
            Path projectPath = Paths.get(BASE_STORAGE_PATH, projectDirName);
            
            if (!Files.exists(projectPath)) {
                Files.createDirectories(projectPath);
            }
            
            return projectPath.toString();
        } catch (IOException e) {
            throw new RuntimeException("Failed to create project directory: " + e.getMessage(), e);
        }
    }

    private String sanitizeFileName(String fileName) {
        // Remove or replace invalid characters
        return fileName.replaceAll("[^a-zA-Z0-9._-]", "_");
    }

    // DTO classes for request and response
    public static class SaveContentRequest {
        private Integer projectId;
        private String projectName;
        private String content;

        // Getters and Setters
        public Integer getProjectId() { return projectId; }
        public void setProjectId(Integer projectId) { this.projectId = projectId; }
        public String getProjectName() { return projectName; }
        public void setProjectName(String projectName) { this.projectName = projectName; }
        public String getContent() { return content; }
        public void setContent(String content) { this.content = content; }
    }

    public static class ProjectFilesResponse {
        private String projectPath;
        private boolean flowExists;
        private boolean codeExists;
        private String flowLastModified;
        private String codeLastModified;
        private long flowSize;
        private long codeSize;

        // Getters and Setters
        public String getProjectPath() { return projectPath; }
        public void setProjectPath(String projectPath) { this.projectPath = projectPath; }
        public boolean isFlowExists() { return flowExists; }
        public void setFlowExists(boolean flowExists) { this.flowExists = flowExists; }
        public boolean isCodeExists() { return codeExists; }
        public void setCodeExists(boolean codeExists) { this.codeExists = codeExists; }
        public String getFlowLastModified() { return flowLastModified; }
        public void setFlowLastModified(String flowLastModified) { this.flowLastModified = flowLastModified; }
        public String getCodeLastModified() { return codeLastModified; }
        public void setCodeLastModified(String codeLastModified) { this.codeLastModified = codeLastModified; }
        public long getFlowSize() { return flowSize; }
        public void setFlowSize(long flowSize) { this.flowSize = flowSize; }
        public long getCodeSize() { return codeSize; }
        public void setCodeSize(long codeSize) { this.codeSize = codeSize; }
    }
}
