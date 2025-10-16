import { useQuery } from "@tanstack/react-query";
import { fetchFeaturedTracks, fetchTracks, fetchFeaturedPlaylists } from "../services/apiClient";

export const useFeaturedTracks = () =>
  useQuery({
    queryKey: ["tracks", "featured"],
    queryFn: fetchFeaturedTracks
  });

export const useTracks = (page: number, pageSize: number) =>
  useQuery({
    queryKey: ["tracks", page, pageSize],
    queryFn: () => fetchTracks(page, pageSize)
  });

export const useFeaturedPlaylists = () =>
  useQuery({
    queryKey: ["playlists", "featured"],
    queryFn: fetchFeaturedPlaylists
  });
