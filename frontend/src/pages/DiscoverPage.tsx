import { Helmet } from "react-helmet-async";
import { Pagination, Row, Col, Skeleton, Empty } from "antd";
import { useState } from "react";
import { TrackCard } from "../components/TrackCard";
import { useTracks } from "../hooks/useTracks";

const PAGE_SIZE = 12;

export const DiscoverPage = () => {
  const [page, setPage] = useState(1);
  const { data, isLoading } = useTracks(page, PAGE_SIZE);

  return (
    <>
      <Helmet>
        <title>Khám phá âm nhạc indie | MusicPlay</title>
        <meta
          name="description"
          content="Khám phá kho nhạc indie chất lượng, được đề xuất thông minh bởi MusicPlay."
        />
      </Helmet>
      <section style={{ marginBottom: 32 }}>
        <h1 className="section-title">Khám phá thư viện nhạc</h1>
        {isLoading ? (
          <Skeleton active paragraph={{ rows: 10 }} />
        ) : data && data.items.length > 0 ? (
          <>
            <Row gutter={[24, 24]}>
              {data.items.map((track) => (
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
            <div style={{ display: "flex", justifyContent: "center", marginTop: 32 }}>
              <Pagination
                current={data.page}
                total={data.total}
                pageSize={data.pageSize}
                showSizeChanger={false}
                onChange={(value) => setPage(value)}
              />
            </div>
          </>
        ) : (
          <Empty description="Chưa có track nào" />
        )}
      </section>
    </>
  );
};
