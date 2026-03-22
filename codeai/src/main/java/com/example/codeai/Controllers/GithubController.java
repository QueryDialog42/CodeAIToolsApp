package com.example.codeai.Controllers;

import java.io.IOException;
import lombok.AllArgsConstructor;
import org.kohsuke.github.GitHub;
import org.kohsuke.github.GitHubBuilder;
import com.example.codeai.Dtos.ProjectDto;
import org.springframework.http.HttpStatus;
import com.example.codeai.Entities.GitTokens;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.kohsuke.github.GHFileNotFoundException;
import org.springframework.web.client.RestTemplate;
import org.kohsuke.github.GHCreateRepositoryBuilder;
import com.example.codeai.Repositories.IUserRepository;
import com.example.codeai.Repositories.IGitTokensRepository;
import org.springframework.web.client.HttpClientErrorException;

@RestController
@AllArgsConstructor
@RequestMapping("/github")
public class GithubController {

    private final IUserRepository userRepository;
    private final IGitTokensRepository gitTokensRepository;


    @GetMapping("/isExist/{github_username}")
    private ResponseEntity<Void> checkIfGithubExist(@PathVariable String github_username) {

        String githubApiUrl = "https://api.github.com/users/" + github_username;

        RestTemplate restTemplate = new RestTemplate();

        try {
            ResponseEntity<String> response = restTemplate.getForEntity(githubApiUrl, String.class);

            if (response.getStatusCode() == HttpStatus.OK) {
                return ResponseEntity.ok().build();
            }

            return ResponseEntity.notFound().build();

        } catch (HttpClientErrorException.NotFound e) {
            return ResponseEntity.notFound().build();

        } catch (Exception e) {
            return ResponseEntity.internalServerError().build();
        }
    }

    @GetMapping("/token/{u_id}")
    private ResponseEntity<String> getGithubToken(@PathVariable Integer u_id){
        var token = gitTokensRepository.findById(u_id);
        return token.map(gitTokens -> ResponseEntity.ok(gitTokens.getGit_token())).orElseGet(() -> ResponseEntity.notFound().build());
    }

    @GetMapping("/token/update/{u_id}")
    private ResponseEntity<Void> updateToken(@PathVariable Integer u_id, @RequestHeader("token") String new_token){
        try {

            saveOrUpdateGitToken(u_id, new_token);

            return ResponseEntity.ok().build();

        } catch (Exception e) {
            return ResponseEntity.internalServerError().build(); // 500
        }

    }

    @GetMapping("/isValid")
    public ResponseEntity<Boolean> isTokenValid(@RequestHeader("token") String token) {
        try {
            GitHub github = new GitHubBuilder()
                    .withOAuthToken(token)
                    .build();

            github.getMyself(); // Token geçersizse burada exception fırlatır

            return ResponseEntity.ok(true);
        } catch (IOException e) {
            return ResponseEntity.ok(false);
        }
    }

    @PostMapping("/create")
    private ResponseEntity<Void> createGitRepo(@RequestBody ProjectDto projectDto, @RequestHeader("token") String githubToken){
        try {
            GitHub github = new GitHubBuilder()
                    .withOAuthToken(githubToken)
                    .build();

            GHCreateRepositoryBuilder repoBuilder = github.createRepository(projectDto.getP_name())
                    .description(projectDto.getP_description())
                    .private_(false)
                    .autoInit(false);

            repoBuilder.create();

            saveOrUpdateGitToken(projectDto.getBelongs_to(), githubToken);

            return ResponseEntity.status(HttpStatus.CREATED).build(); // 201
        } catch (GHFileNotFoundException e) {
            return ResponseEntity.notFound().build(); // 404
        } catch (IOException e) {
            return ResponseEntity.internalServerError().build(); // 500
        }
    }

    @DeleteMapping("/delete/{p_name}")
    public ResponseEntity<Void> deleteGitRepo(@PathVariable String p_name, @RequestHeader("user_id") String u_id){
        Integer user_id = Integer.parseInt(u_id);
        var githubToken = gitTokensRepository.findById(user_id);

        if (githubToken.isEmpty()) return ResponseEntity.notFound().build();

        try {
            GitHub github = new GitHubBuilder()
                    .withOAuthToken(githubToken.get().getGit_token())
                    .build();

            String username = github.getMyself().getLogin();
            github.getRepository(username + "/" + p_name).delete();

            return ResponseEntity.noContent().build(); // 204
        } catch (GHFileNotFoundException e) {
            return ResponseEntity.notFound().build(); // 404
        } catch (IOException e) {
            return ResponseEntity.internalServerError().build(); // 500
        }
    }

    private void saveOrUpdateGitToken(Integer u_id, String git_token) {
        var existingToken = gitTokensRepository.findById(u_id);

        if (existingToken.isPresent()) {
            GitTokens token = existingToken.get();
            token.setGit_token(git_token);
            gitTokensRepository.save(token);
        } else {
            var user = userRepository.findById(u_id);
            if (user.isEmpty()) return;

            GitTokens token = new GitTokens();
            token.setUser(user.get());  // ← @MapsId için user set edilmeli
            token.setGit_token(git_token);
            gitTokensRepository.save(token);
        }
    }
}
