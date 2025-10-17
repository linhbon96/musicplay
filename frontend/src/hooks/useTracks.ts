import { useQuery } from "@tanstack/react-query";
import {
  fetchFeaturedTracks,
  fetchTracks,
  fetchFeaturedPlaylists,
  fetchPersonalizedTracks,
  fetchAdminOverview
} from "../services/apiClient";

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

export const usePersonalizedTracks = () =>
  useQuery({
    queryKey: ["tracks", "personalized"],
    queryFn: fetchPersonalizedTracks
  });

export const useAdminOverview = () =>
  useQuery({
    queryKey: ["admin", "overview"],
    queryFn: fetchAdminOverview
  });
