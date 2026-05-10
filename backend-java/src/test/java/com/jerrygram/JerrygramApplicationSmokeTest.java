package com.jerrygram;

import com.jerrygram.domain.enums.PostVisibility;
import com.jerrygram.domain.valueobjects.PostCaption;
import org.junit.jupiter.api.Test;
import org.springframework.boot.autoconfigure.SpringBootApplication;

import static org.assertj.core.api.Assertions.assertThat;

class JerrygramApplicationSmokeTest {

    @Test
    void applicationClassKeepsSpringBootEntryPoint() {
        assertThat(JerrygramApplication.class.getAnnotation(SpringBootApplication.class)).isNotNull();
    }

    @Test
    void postVisibilityOrdinalContractMatchesDatabaseValues() {
        assertThat(PostVisibility.Public.ordinal()).isZero();
        assertThat(PostVisibility.FollowersOnly.ordinal()).isEqualTo(1);
        assertThat(PostVisibility.Private.ordinal()).isEqualTo(2);
    }

    @Test
    void postCaptionExtractsHashtagsAndMentions() {
        PostCaption caption = PostCaption.create("Learning #Kafka #DotNet with @Jerry");

        assertThat(caption.getHashtags()).containsExactly("kafka", "dotnet");
        assertThat(caption.getMentions()).containsExactly("jerry");
    }
}
