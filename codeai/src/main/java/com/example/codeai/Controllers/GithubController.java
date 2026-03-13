package com.example.codeai.Controllers;

import com.example.codeai.Dtos.ProjectDto;
import com.example.codeai.Entities.GitTokens;
import com.example.codeai.Repositories.IGitTokensRepository;
import com.example.codeai.Repositories.IUserRepository;
import lombok.AllArgsConstructor;
import org.kohsuke.github.GHCreateRepositoryBuilder;
import org.kohsuke.github.GHFileNotFoundException;
import org.kohsuke.github.GitHub;
import org.kohsuke.github.GitHubBuilder;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.io.IOException;

@AllArgsConstructor
@RestController
@RequestMapping("/github")
public class GithubController {

    private final IGitTokensRepository gitTokensRepository;
    private final IUserRepository userRepository;

    @GetMapping("/token/{u_id}")
    private ResponseEntity<String> getGithubToken(@PathVariable Integer u_id){
        var token = gitTokensRepository.findById(u_id);
        return token.map(gitTokens -> ResponseEntity.ok(gitTokens.getGit_token())).orElseGet(() -> ResponseEntity.notFound().build());
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

            saveGitTokenIfNotExist(githubToken, projectDto.getBelongs_to());

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

    private void saveGitTokenIfNotExist(String gitToken, Integer belongs_to) {
        var git_token = gitTokensRepository.findToken(gitToken);

        if (git_token.isEmpty()) {
            var user = userRepository.findById(belongs_to).orElse(null);
            if (user == null) return;

            var savedToken = new GitTokens();
            savedToken.setUser(user);        // Users nesnesini set et
            savedToken.setGit_token(gitToken);

            gitTokensRepository.save(savedToken);
        }
    }
}
