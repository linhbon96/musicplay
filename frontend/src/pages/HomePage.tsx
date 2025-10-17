import { Helmet } from "react-helmet-async";
import { Row, Col, Skeleton, Empty, Typography, Card } from "antd";
import { HeroBanner } from "../components/HeroBanner";
import { StatsBar } from "../components/StatsBar";
import { TrackCard } from "../components/TrackCard";
import { PlaylistShowcase } from "../components/PlaylistShowcase";
import { useFeaturedTracks, useFeaturedPlaylists, usePersonalizedTracks } from "../hooks/useTracks";

const { Text } = Typography;

export const HomePage = () => {
  const { data: featuredTracks, isLoading: tracksLoading } = useFeaturedTracks();
  const { data: playlists, isLoading: playlistsLoading } = useFeaturedPlaylists();
  const { data: personalizedTracks, isLoading: personalizedLoading } = usePersonalizedTracks();

  const showPersonalized = personalizedTracks && personalizedTracks.length > 0;

  return (
    <>
      <Helmet>
        <title>MusicPlay · Nghe & chia sẻ âm nhạc indie</title>
        <meta
          name="description"
          content="MusicPlay hỗ trợ nghệ sĩ indie đăng tải nhạc, stream mượt với chuẩn 206 và gợi ý playlist thông minh."
        />
      </Helmet>
      <HeroBanner />
      <StatsBar />

      {showPersonalized && (
        <section style={{ marginBottom: 48 }}>
          <h2 className="section-title">Gợi ý dành riêng cho bạn</h2>
          {personalizedLoading ? (
            <Skeleton active paragraph={{ rows: 6 }} />
          ) : (
            <Row gutter={[24, 24]}>
              {personalizedTracks!.map((track) => (
                <Col xs={24} sm={12} md={8} lg={6} key={track.id}>
                  <TrackCard
                    title={track.title}
                    artistName={track.artistName}
                    description={track.description}
                    streamUrl={track.streamUrl}
                    coverUrl={track.coverUrl}
                    featured={track.featured}
                  />
                </Col>
              ))}
            </Row>
          )}
        </section>
      )}

      {!showPersonalized && personalizedLoading && (
        <section style={{ marginBottom: 48 }}>
          <h2 className="section-title">Gợi ý dành riêng cho bạn</h2>
          <Skeleton active paragraph={{ rows: 6 }} />
        </section>
      )}

      {!showPersonalized && !personalizedLoading && (
        <section style={{ marginBottom: 48 }}>
          <Card>
            <Text>
              Đăng nhập và nghe một vài track để MusicPlay học sở thích của bạn và đưa ra gợi ý phù hợp hơn.
            </Text>
          </Card>
        </section>
      )}

      <section style={{ marginBottom: 48 }}>
        <h2 className="section-title">Track nổi bật tuần này</h2>
        {tracksLoading ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : featuredTracks && featuredTracks.length > 0 ? (
          <Row gutter={[24, 24]}>
            {featuredTracks.map((track) => (
              <Col xs={24} sm={12} md={8} lg={6} key={track.id}>
                <TrackCard
                  title={track.title}
                  artistName={track.artistName}
                  description={track.description}
                  streamUrl={track.streamUrl}
                  coverUrl={track.coverUrl}
                  featured={track.featured}
                />
              </Col>
            ))}
          </Row>
        ) : (
          <Empty description="Chưa có track nổi bật" />
        )}
      </section>

      <section>
        <h2 className="section-title">Playlist được đề xuất</h2>
        {playlistsLoading ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : playlists && playlists.length > 0 ? (
          <Row gutter={[24, 24]}>
            {playlists.map((playlist) => (
              <Col xs={24} md={12} key={playlist.id}>
                <PlaylistShowcase
                  title={playlist.title}
                  description={playlist.description}
                  coverUrl={playlist.coverUrl}
                  tracks={playlist.tracks.map((track) => ({
                    id: track.id,
                    title: track.title,
                    artistName: track.artistName,
                    streamUrl: track.streamUrl
                  }))}
                />
              </Col>
            ))}
          </Row>
        ) : (
          <Empty description="Chưa có playlist đề xuất" />
        )}
      </section>
    </>
  );
};
