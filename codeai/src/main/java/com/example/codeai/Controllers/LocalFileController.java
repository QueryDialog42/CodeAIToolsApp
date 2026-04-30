package com.example.codeai.Controllers;

import lombok.AllArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

@RestController
@AllArgsConstructor
@RequestMapping("/local")
public class LocalFileController {

    private static final String BASE_STORAGE_PATH = "CodeAI_Root";
    private static final String FLOW_FILE = "Flow.txt";
    private static final String CODE_FILE = "Code.txt";

    @PostMapping("/save/flow")
    public ResponseEntity<String> saveFlowContent(@RequestBody SaveContentRequest request) {
        try {
            // Create project directory if it doesn't exist
            String projectPath = createProjectDirectory(request.getProjectId(), request.getProjectName());
            
            // Create Flow.txt file path
            Path flowFilePath = Paths.get(projectPath, FLOW_FILE);
            
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
            // Create project directory if it doesn't exist
            String projectPath = createProjectDirectory(request.getProjectId(), request.getProjectName());
            
            // Create Code.txt file path
            Path codeFilePath = Paths.get(projectPath, CODE_FILE);
            
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
            String projectPath = createProjectDirectory(projectId, projectName);
            Path flowFilePath = Paths.get(projectPath, FLOW_FILE);
            
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
            String projectPath = createProjectDirectory(projectId, projectName);
            Path codeFilePath = Paths.get(projectPath, CODE_FILE);
            
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
