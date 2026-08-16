import { TechPackView } from "@/components/patterns/TechPackView";

export default async function TechPackPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <TechPackView patternId={id} />;
}
