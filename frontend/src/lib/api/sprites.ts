import { apiGet, type ApiResult } from './client';

export type SpriteDto = {
	id: string;
	name: string;
	type: string;
	description: string;
	assetPath: string;
	layer: number;
	archetypeFamily: string;
	isDefault: boolean;
};

export type ListSpritesResponse = {
	sprites: SpriteDto[];
};

export type ListSpritesFilter = {
	type?: string;
	archetypeFamily?: string;
};

export const spritesApi = {
	list(filter: ListSpritesFilter = {}): Promise<ApiResult<ListSpritesResponse>> {
		const params = new URLSearchParams();
		if (filter.type) params.set('type', filter.type);
		if (filter.archetypeFamily) params.set('archetypeFamily', filter.archetypeFamily);

		const queryString = params.toString();
		const path = queryString ? `/api/sprites?${queryString}` : '/api/sprites';
		return apiGet<ListSpritesResponse>(path);
	}
};